namespace Project.Areas.Admin.Models.DTOs
{
    public class MedicineCategoryDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public IFormFile? ImageFile { get; set; }
    }
}
