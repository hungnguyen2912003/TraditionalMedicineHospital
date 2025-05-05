using System.ComponentModel.DataAnnotations;

namespace Project.Models.Enums
{
    public enum TrackingStatus
    {
        [Display(Name = "Có khám")]
        Present = 1,
        [Display(Name = "Đã xin phép")]
        Excused = 2,
        [Display(Name = "Không khám")]
        Absent = 3
    }
}
