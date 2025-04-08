namespace Project.Areas.Admin.Models.DTOs
{
    public class UserDto
    {
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public Guid EmployeeId { get; set; }
    }
}
