using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System;

namespace SupertronicsRepairSystem.ViewModels.Technician
{
    // ViewModel for generating a repair quote
    public class GenerateRepairQuoteViewModel
    {
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
        public string? CustomerEmail { get; set; }

        [Display(Name = "Customer Phone")]
        public string? CustomerPhone { get; set; }

        [Display(Name = "Brand/Manufacturer")]
        public string? Brand { get; set; }

        [Display(Name = "Model")]
        public string? Model { get; set; }

        [Display(Name = "Diagnosis Notes")]
        public string? DiagnosisNotes { get; set; }

        public List<QuotePartItem> Parts { get; set; } = new List<QuotePartItem>(); // Parts to be included in the quote

        [Display(Name = "Labor Hours")]
        public decimal LaborHours { get; set; } = 1.0m;

        [Display(Name = "Labor Rate (R/hour)")]
        public decimal LaborRate { get; set; } = 750m;

        [Display(Name = "Quote Valid Until")]
        [DataType(DataType.Date)]
        public DateTime ValidUntil { get; set; } = DateTime.Now.AddDays(30);

        // Calculated properties
        public decimal PartsTotal => Parts?.Sum(p => p.TotalPrice) ?? 0;
        public decimal LaborTotal => LaborHours * LaborRate;
        public decimal SubTotal => PartsTotal + LaborTotal;
        public decimal TaxRate { get; set; } = 0.15m; // 15% VAT rate
        public decimal TaxAmount => SubTotal * TaxRate;
        public decimal Total => SubTotal + TaxAmount;
    }

    // Represents a part item in the quote
    public class QuotePartItem
    {
        [Display(Name = "Part Name")]
        public string? PartName { get; set; }

        [Display(Name = "Part Number")]
        public string? PartNumber { get; set; }

        public string? Description { get; set; }

        public int Quantity { get; set; } = 1;

        [Display(Name = "Unit Price")]
        public decimal UnitPrice { get; set; }

        public decimal TotalPrice => Quantity * UnitPrice;
    }
}