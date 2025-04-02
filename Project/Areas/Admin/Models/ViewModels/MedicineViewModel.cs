namespace Project.Areas.Admin.Models.ViewModels
{
    public class MedicineViewModel
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int StockUnit { get; set; }
        public string Manufacturer { get; set; }
        public DateTime ManufacturedDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsActive { get; set; }
        public string CategoryName { get; set; }
    }
}
