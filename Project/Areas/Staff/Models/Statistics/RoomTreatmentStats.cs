using System;

namespace Project.Areas.Staff.Models.Statistics
{
    public class RoomTreatmentStats
    {
        public Guid RoomId { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public int TreatmentCount { get; set; }
        public DateTime Date { get; set; }
    }
} 