using Project.Models.Enums;

namespace Project.Areas.Admin.Models.ViewModels
{
    public class UserViewModel
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public RoleType Role { get; set; }
        public string? EmployeeName { get; set; }
        public bool IsActive { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}