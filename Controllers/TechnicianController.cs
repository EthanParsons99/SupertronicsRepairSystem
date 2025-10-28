using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SupertronicsRepairSystem.Services;
using SupertronicsRepairSystem.ViewModels.Technician;
using SupertronicsRepairSystem.Data.Models;
using Microsoft.Extensions.Logging;

namespace SupertronicsRepairSystem.Controllers
{
    public class TechnicianController : Controller
    {
        private readonly ITechnicianService _technicianService;
        private readonly ILogger<TechnicianController> _logger;

        public TechnicianController(ITechnicianService technicianService, ILogger<TechnicianController> logger)
        {
            _technicianService = technicianService;
            _logger = logger;
        }

        public async Task<IActionResult> Dashboard()
        {
            var model = await _technicianService.GetDashboardDataAsync();
            return View(model);
        }

        public async Task<IActionResult> RepairJobs(string status, string customer, DateTime? date)
        {
            var repairJobs = await _technicianService.GetFilteredRepairJobsAsync(status, customer, date);
            var viewModels = repairJobs.Select(job => new RepairJobViewModel
            {
                Id = job.Id,
                ItemName = job.ItemModel,
                Status = job.Status,
                CustomerName = job.CustomerName,
                LastUpdated = job.LastUpdated.ToDateTime()
            }).ToList();

            ViewData["SelectedStatus"] = status;
            ViewData["SelectedCustomer"] = customer;
            ViewData["SelectedDate"] = date?.ToString("yyyy-MM-dd");

            return View(viewModels);
        }

        public IActionResult CreateRepairJob() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRepairJob(CreateRepairJobViewModel model)
        {
            if (ModelState.IsValid)
            {
                await _technicianService.CreateRepairJobAsync(model);
                TempData["SuccessMessage"] = $"New repair job for '{model.CustomerName}' created successfully.";
                return RedirectToAction(nameof(RepairJobs));
            }
            return View(model);
        }

        public async Task<IActionResult> SelectJobForQuote()
        {
            var openJobs = await _technicianService.GetFilteredRepairJobsAsync(null, null, null);
            var model = new SelectJobForQuoteViewModel
            {
                OpenJobs = openJobs
                    .Where(j => j.Status != "Completed" && j.Status != "Cancelled")
                    .Select(job => new SelectListItem
                    {
                        Value = job.Id,
                        Text = $"#{job.Id.Substring(0, 6).ToUpper()} - {job.ItemModel} ({job.CustomerName})"
                    }).ToList()
            };
            return View(model);
        }

        public async Task<IActionResult> GenerateRepairQuote(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction(nameof(SelectJobForQuote));
            }
            var repairJob = await _technicianService.GetRepairJobByIdAsync(id);
            if (repairJob == null) return NotFound();

            var model = new GenerateRepairQuoteViewModel
            {
                JobId = repairJob.Id,
                CustomerName = repairJob.CustomerName,
                DeviceName = repairJob.ItemModel,
                SerialNumber = repairJob.SerialNumber,
                ProblemDescription = repairJob.ProblemDescription
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateRepairQuote(GenerateRepairQuoteViewModel model)
        {
            _logger.LogInformation("--- GenerateRepairQuote [HttpPost] action initiated. ---");
            _logger.LogInformation($"Attempting to generate quote for JobId: {model?.JobId}");

            if (model == null)
            {
                _logger.LogError("Model object is null. Cannot proceed.");
                TempData["ErrorMessage"] = "An unexpected error occurred. The submitted data was empty.";
                return RedirectToAction(nameof(RepairJobs));
            }

            if (ModelState.IsValid)
            {
                _logger.LogInformation("ModelState is VALID. Proceeding to save quote data.");
                try
                {
                    var success = await _technicianService.AddQuoteToRepairJobAsync(model.JobId, model);
                    if (success)
                    {
                        _logger.LogInformation("Service call to AddQuoteToRepairJobAsync SUCCEEDED. Redirecting with success message.");
                        TempData["SuccessMessage"] = $"Quote added to job #{model.JobId.Substring(0, 6).ToUpper()}.";
                    }
                    else
                    {
                        _logger.LogWarning("Service call to AddQuoteToRepairJobAsync FAILED. The service returned false. Redirecting with error message.");
                        TempData["ErrorMessage"] = "Error: Could not find the job to add the quote to.";
                    }
                    return RedirectToAction(nameof(RepairJobs));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An exception occurred while trying to save the quote.");
                    TempData["ErrorMessage"] = "A critical error occurred while saving the quote.";
                    return RedirectToAction(nameof(RepairJobs));
                }
            }

            _logger.LogWarning("ModelState is INVALID. Reloading the view with validation errors.");
            foreach (var key in ModelState.Keys)
            {
                var state = ModelState[key];
                if (state.Errors.Any())
                {
                    _logger.LogWarning($"-- Validation Error for '{key}': {state.Errors.First().ErrorMessage}");
                }
            }

            return View(model);
        }

        public async Task<IActionResult> UpdateRepairStatus(string id)
        {
            var job = await _technicianService.GetRepairJobByIdAsync(id);
            if (job == null) return NotFound();

            var model = new UpdateRepairStatusViewModel
            {
                JobId = job.Id,
                ItemModel = job.ItemModel,
                CustomerName = job.CustomerName,
                CurrentStatus = job.Status
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRepairStatus(UpdateRepairStatusViewModel model)
        {
            if (ModelState.IsValid)
            {
                var success = await _technicianService.UpdateRepairJobStatusAsync(model.JobId, model.NewStatus);
                if (success)
                {
                    TempData["SuccessMessage"] = $"Status for job #{model.JobId.Substring(0, 6).ToUpper()} updated to '{model.NewStatus}'.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Error: Could not find the job to update.";
                }
                return RedirectToAction(nameof(RepairJobs));
            }

            model.StatusOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "Pending", Text = "Pending" },
                new SelectListItem { Value = "Diagnosis", Text = "Diagnosis" },
                new SelectListItem { Value = "In Progress", Text = "In Progress" },
                new SelectListItem { Value = "Completed", Text = "Completed" },
                new SelectListItem { Value = "Cancelled", Text = "Cancelled" }
            };
            return View(model);
        }

        public async Task<IActionResult> AddRepairNotes(string id)
        {
            var job = await _technicianService.GetRepairJobByIdAsync(id);
            if (job == null) return NotFound();

            var model = new AddRepairNotesViewModel
            {
                JobId = job.Id,
                ItemModel = job.ItemModel,
                CustomerName = job.CustomerName,
                ExistingNotes = job.TechnicianNotes.OrderByDescending(n => n.Timestamp).ToList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRepairNotes(AddRepairNotesViewModel model)
        {
            if (ModelState.IsValid)
            {
                var success = await _technicianService.AddNoteToRepairJobAsync(model.JobId, model.NewNote);
                if (success)
                {
                    TempData["SuccessMessage"] = $"Note added to job #{model.JobId.Substring(0, 6).ToUpper()}.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Error: Could not find the job to add a note to.";
                }
                return RedirectToAction(nameof(RepairJobs));
            }

            var job = await _technicianService.GetRepairJobByIdAsync(model.JobId);
            if (job != null)
            {
                model.ExistingNotes = job.TechnicianNotes.OrderByDescending(n => n.Timestamp).ToList();
            }
            return View(model);
        }

        public IActionResult TrackSerialNumber() => View(new TrackSerialNumberViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TrackSerialNumber(TrackSerialNumberViewModel model)
        {
            if (!string.IsNullOrEmpty(model.SerialNumberToSearch))
            {
                model.FoundJobs = await _technicianService.FindRepairJobsBySerialNumberAsync(model.SerialNumberToSearch);
            }
            model.SearchPerformed = true;
            return View(model);
        }

        public IActionResult GenerateProductQuote() => View(new GenerateProductQuoteViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateProductQuote(GenerateProductQuoteViewModel model)
        {
            if (ModelState.IsValid)
            {
                var productName = await _technicianService.CreateProductQuoteAsync(model);
                TempData["SuccessMessage"] = $"Product quote for '{productName}' created successfully!";
                return RedirectToAction(nameof(Dashboard));
            }
            return View(model);
        }
    }
}