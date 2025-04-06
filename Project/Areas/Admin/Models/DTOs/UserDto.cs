namespace Project.Areas.Admin.Models.DTOs
{
    public class UserDto
    {
        public string Code { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }
        public Guid EmployeeId { get; set; }
    }
}
