using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SupertronicsRepairSystem.Controllers;
using SupertronicsRepairSystem.Models;
using SupertronicsRepairSystem.Services;
using SupertronicsRepairSystem.ViewModels.Technician;
using System;
using System.Threading.Tasks;

namespace SupertronicsRepairSystem.Tests
{
    // Unit tests for the TechnicianController assissted by (Gemini, 2025)
    [TestClass]
    public class TechnicianControllerTests
    {
        // These are our mock objects. We use interfaces so Moq can create fake versions of them.
        private Mock<IRepairJobService> _mockRepairJobService;
        private Mock<ILogger<TechnicianController>> _mockLogger;
        private Mock<IAuthService> _mockAuthService;

        // This is the actual class we are testing.
        private TechnicianController _controller;

        // This method runs before each test to ensure a clean, isolated environment.
        [TestInitialize]
        public void TestInitialize()
        {
            // Create fresh mocks for every test.
            _mockRepairJobService = new Mock<IRepairJobService>();
            _mockLogger = new Mock<ILogger<TechnicianController>>();
            _mockAuthService = new Mock<IAuthService>();

            // Instantiate the controller, passing null for FirestoreDb as our tested methods don't use it directly.
            _controller = new TechnicianController(
                _mockRepairJobService.Object,
                null,
                _mockLogger.Object
            );


            // Mock the HttpContext and its service provider to handle dependencies like IAuthService.
            var httpContext = new DefaultHttpContext();
            var services = new Mock<IServiceProvider>();
            services.Setup(s => s.GetService(typeof(IAuthService))).Returns(_mockAuthService.Object);
            httpContext.RequestServices = services.Object;
            _controller.ControllerContext = new ControllerContext() { HttpContext = httpContext };

            // Provide a default setup for the auth service to prevent crashes in the layout.
            _mockAuthService.Setup(s => s.GetCurrentUserInfoAsync()).ReturnsAsync((UserInfo)null);

            // Mock TempData so the controller can write success/error messages without crashing.
            var tempDataFactory = new TempDataDictionaryFactory(new Mock<ITempDataProvider>().Object);
            _controller.TempData = tempDataFactory.GetTempData(httpContext);

            // Mock the URL Helper so that RedirectToAction() can work without crashing.
            var mockUrlHelper = new Mock<IUrlHelper>();
            mockUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns("a/fake/url");
            _controller.Url = mockUrlHelper.Object;
        }

        [TestMethod]
        public async Task CreateRepairJob_WhenModelStateIsValid_CallsServiceAndRedirects()
        {
            // ARRANGE

            // Create the input data that simulates what the form would send.
            var viewModel = new CreateRepairJobViewModel
            {
                CustomerName = "John Doe",
                ItemModel = "Laptop XYZ"
            };

            // Set up the mock service's behavior
            _mockRepairJobService
                .Setup(service => service.CreateRepairJobAsync(viewModel, " ", viewModel.CustomerName))
                .ReturnsAsync("new-job-id-123");

            // ACT: Execute the controller action we want to test.
            var result = await _controller.CreateRepairJob(viewModel);

            // ASSERT: Verify that the outcome is what we expected.

            // Verify that the CreateRepairJobAsync method on our mock service was called exactly once
            _mockRepairJobService.Verify(service =>
                service.CreateRepairJobAsync(viewModel, " ", viewModel.CustomerName),
                Times.Once);

            // Assert that the result of the action is a RedirectToAction.
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirectResult = result as RedirectToActionResult;

            // Assert that it redirects to the correct page ("RepairJobs").
            Assert.AreEqual("RepairJobs", redirectResult.ActionName);

            // Assert that a success message was set in TempData.
            Assert.IsNotNull(_controller.TempData["SuccessMessage"]);
        }

        [TestMethod]
        public async Task GenerateRepairQuote_WhenJobExists_ReturnsViewWithCorrectData()
        {
            // ARRANGE
            var testJobId = "job-abc-123";
            var repairJobFromService = new RepairJob
            {
                Id = testJobId,
                CustomerName = "Jane Smith",
                ItemModel = "Smartphone"
            };

            // Set up the mock service to return our fake RepairJob object when GetRepairJobByIdAsync is called.
            _mockRepairJobService
                .Setup(service => service.GetRepairJobByIdAsync(testJobId))
                .ReturnsAsync(repairJobFromService);

            // ACT
            var result = await _controller.GenerateRepairQuote(testJobId);

            // ASSERT

            // Assert that the action returned a ViewResult.
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = result as ViewResult;

            // Assert that the model inside the ViewResult is of the correct ViewModel type.
            Assert.IsInstanceOfType(viewResult.Model, typeof(GenerateRepairQuoteViewModel));
            var model = viewResult.Model as GenerateRepairQuoteViewModel;

            // Assert that the data was correctly mapped from the service's data model to the view model.
            Assert.AreEqual(testJobId, model.JobId);
            Assert.AreEqual("Jane Smith", model.CustomerName);
            Assert.AreEqual("Smartphone", model.DeviceName);
        }
    }
}