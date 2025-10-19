using System.ComponentModel.DataAnnotations;

namespace SupertronicsRepairSystem.ViewModels.Technician {
    public class GenerateRepairQuoteViewModel {
        public int JobId { get; set; }
        
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerPhone { get; set; }
        
        [Required]
        [Display(Name = "Device/Item")]
        public string? DeviceName { get; set; }
        
        [Display(Name = "Serial Number")]
        public string? SerialNumber { get; set; }
        
        [Display(Name = "Brand/Manufacturer")]
        public string? Brand { get; set; }
        
        [Display(Name = "Model")]
        public string? Model { get; set; }
        
        [Required]
        [Display(Name = "Problem Description")]
        [StringLength(500, ErrorMessage = "Problem description cannot exceed 500 characters")]
        public string? ProblemDescription { get; set; }
        
        [Display(Name = "Diagnosis Notes")]
        [StringLength(1000, ErrorMessage = "Diagnosis notes cannot exceed 1000 characters")]
        public string? DiagnosisNotes { get; set; }
        
        [Required]
        [Display(Name = "Required Parts")]
        public List<QuotePartItem> Parts { get; set; } = new List<QuotePartItem>();
        
        [Required]
        [Display(Name = "Labor Hours")]
        [Range(0.25, 100, ErrorMessage = "Labor hours must be between 0.25 and 100")]
        public decimal LaborHours { get; set; }
        
        [Required]
        [Display(Name = "Labor Rate (R/hour)")]
        [Range(200, 2000, ErrorMessage = "Labor rate must be between R200 and R2000 per hour")]
        public decimal LaborRate { get; set; } = 750m; 
        
        public decimal PartsTotal => Parts?.Sum(p => p.Quantity * p.UnitPrice) ?? 0;
        public decimal LaborTotal => LaborHours * LaborRate;
        public decimal SubTotal => PartsTotal + LaborTotal;
        public decimal TaxRate { get; set; } = 0.15m; 
        public decimal TaxAmount => SubTotal * TaxRate;
        public decimal Total => SubTotal + TaxAmount;
        
        [Display(Name = "Estimated Completion Time")]
        public int EstimatedDays { get; set; } = 3;
        
        [Display(Name = "Quote Valid Until")]
        public DateTime ValidUntil { get; set; } = DateTime.Now.AddDays(30);
        
        [Display(Name = "Warranty Period (days)")]
        [Range(30, 365, ErrorMessage = "Warranty period must be between 30 and 365 days")]
        public int WarrantyDays { get; set; } = 90;
        
        [Display(Name = "Additional Notes")]
        [StringLength(1000, ErrorMessage = "Additional notes cannot exceed 1000 characters")]
        public string? AdditionalNotes { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
    }
    
    public class QuotePartItem {
        [Required]
        [Display(Name = "Part Name")]
        public string? PartName { get; set; }
        
        [Display(Name = "Part Number")]
        public string? PartNumber { get; set; }
        
        [Display(Name = "Description")]
        public string? Description { get; set; }
        
        [Required]
        [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100")]
        public int Quantity { get; set; } = 1;
        
        [Required]
        [Display(Name = "Unit Price")]
        [Range(0.01, 100000, ErrorMessage = "Unit price must be between R0.01 and R100,000")]
        public decimal UnitPrice { get; set; }
        
        public decimal TotalPrice => Quantity * UnitPrice;
    }
}
