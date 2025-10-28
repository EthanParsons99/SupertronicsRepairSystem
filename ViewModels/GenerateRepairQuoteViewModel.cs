using System.ComponentModel.DataAnnotations;

namespace SupertronicsRepairSystem.ViewModels.Technician
{
    public class GenerateRepairQuoteViewModel
    {
        [Required]
        public string JobId { get; set; }

        [Display(Name = "Customer Name")]
        public string? CustomerName { get; set; }

        [Display(Name = "Device/Item")]
        public string? DeviceName { get; set; }

        [Display(Name = "Serial Number")]
        public string? SerialNumber { get; set; }

        [Display(Name = "Problem Description")]
        public string? ProblemDescription { get; set; }

        [Display(Name = "Customer Email")]
        [EmailAddress]
        public string? CustomerEmail { get; set; }

        [Display(Name = "Customer Phone")]
        [Phone]
        public string? CustomerPhone { get; set; }

        [Display(Name = "Brand/Manufacturer")]
        public string? Brand { get; set; }

        [Display(Name = "Model")]
        public string? Model { get; set; }

        [Display(Name = "Diagnosis Notes")]
        [StringLength(1000)]
        public string? DiagnosisNotes { get; set; }

        public List<QuotePartItem> Parts { get; set; } = new List<QuotePartItem>();

        [Required]
        [Display(Name = "Labor Hours")]
        [Range(0.25, 100)]
        public decimal LaborHours { get; set; }

        [Required]
        [Display(Name = "Labor Rate (R/hour)")]
        [Range(200, 2000)]
        public decimal LaborRate { get; set; } = 750m;

        [Display(Name = "Quote Valid Until")]
        [DataType(DataType.Date)]
        public DateTime ValidUntil { get; set; } = DateTime.Now.AddDays(30);

        // Calculated properties
        public decimal PartsTotal => Parts?.Sum(p => p.TotalPrice) ?? 0;
        public decimal LaborTotal => LaborHours * LaborRate;
        public decimal SubTotal => PartsTotal + LaborTotal;
        public decimal TaxRate { get; set; } = 0.15m;
        public decimal TaxAmount => SubTotal * TaxRate;
        public decimal Total => SubTotal + TaxAmount;
    }

    public class QuotePartItem
    {
        [Required]
        [Display(Name = "Part Name")]
        public string? PartName { get; set; }

        [Display(Name = "Part Number")]
        public string? PartNumber { get; set; }

        public string? Description { get; set; }

        [Required]
        [Range(1, 100)]
        public int Quantity { get; set; } = 1;

        [Required]
        [Display(Name = "Unit Price")]
        [Range(0.01, 100000)]
        public decimal UnitPrice { get; set; }

        public decimal TotalPrice => Quantity * UnitPrice;
    }
}