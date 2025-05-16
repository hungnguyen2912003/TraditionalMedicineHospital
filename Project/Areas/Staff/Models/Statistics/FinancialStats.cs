using System;

namespace Project.Areas.Staff.Models.Statistics
{
    public class FinancialStats
    {
        public decimal TotalIncome { get; set; }
        public int PrescriptionCount { get; set; }
        public DateTime Date { get; set; }
    }
} 