using Project.Models.Enums;

namespace Project.Areas.Admin.Models.DTOs
{
    public class MedicineDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public IFormFile? ImageFile { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public UnitType StockUnit { get; set; }
        public ManufacturerType Manufacturer { get; set; }
        public DateTime ManufacturedDate { get; set; }
        public string ActiveIngredient { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public Guid MedicineCategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
