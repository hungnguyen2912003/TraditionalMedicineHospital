namespace Project.Areas.Staff.Models.DTOs
{
    public class PatientDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string IdentityNumber { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string? EmailAddress { get; set; }
        public IFormFile? ImageFile { get; set; }
    }
}
