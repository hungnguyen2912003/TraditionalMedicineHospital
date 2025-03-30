using Project.Areas.Admin.Models.Enums.Medicines;

namespace Project.Areas.Admin.Models.DTOs
{
    public class MedicineDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Images { get; set; }
        public IFormFile? ImageFile { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public UnitType StockUnit { get; set; }
        public string Manufacturer { get; set; }
        public DateTime ManufacturedDate { get; set; }
        public string ActiveIngredient { get; set; }
        public DateTime ExpiryDate { get; set; }
        public Guid MedicineCategoryId { get; set; }
        public string CategoryName { get; set; }
        public bool IsActive { get; set; }
    }
}
