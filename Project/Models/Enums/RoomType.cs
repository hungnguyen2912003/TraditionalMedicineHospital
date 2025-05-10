using System.ComponentModel.DataAnnotations;

namespace Project.Models.Enums
{
    public enum RoomType
    {
        [Display(Name = "Phòng hành chính")]
        Administrative = 0,
        [Display(Name = "Phòng khám")]
        Treatment = 1
    }
}

