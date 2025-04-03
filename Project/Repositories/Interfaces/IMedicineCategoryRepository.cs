using Project.Areas.Admin.Models.Entities;

namespace Project.Repositories.Interfaces
{
    public interface IMedicineCategoryRepository : IBaseRepository<MedicineCategory>
    {
        Task<MedicineCategory?> GetByNameAsync(string name);
        Task<MedicineCategory?> GetByCodeAsync(string code);
    }
}
