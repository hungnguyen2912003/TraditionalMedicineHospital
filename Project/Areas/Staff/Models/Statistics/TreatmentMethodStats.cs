using System;

namespace Project.Areas.Staff.Models.Statistics
{
    public class TreatmentMethodStats
    {
        public Guid TreatmentMethodId { get; set; }
        public string TreatmentMethodName { get; set; } = string.Empty;
        public int TreatmentCount { get; set; }
        public DateTime Date { get; set; }
    }
} 