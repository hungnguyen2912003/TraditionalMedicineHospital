using System;

namespace Project.Areas.Staff.Models.Statistics
{
    public class PatientStats
    {
        public int TotalPatients { get; set; }
        public int WarningPatients { get; set; }
        public DateTime Date { get; set; }
    }
} 