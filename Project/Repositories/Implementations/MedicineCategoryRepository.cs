using Microsoft.EntityFrameworkCore;
using Project.Areas.Admin.Data;
using Project.Areas.Admin.Models.Entities;
using Project.Repositories.Interfaces;

namespace Project.Repositories.Implementations
{
    public class MedicineCategoryRepository : BaseRepository<MedicineCategory>, IMedicineCategoryRepository
    {
        public MedicineCategoryRepository(TraditionalMedicineHospitalDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<MedicineCategory>> GetAllActiveAsync()
        {
            return await _context.medicineCategories
                .Where(mc => mc.IsActive)
                .ToListAsync();
        }

        public async Task<MedicineCategory?> GetByNameAsync(string name)
        {
            return await _context.medicineCategories
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Name == name);
        }

        public async Task<MedicineCategory?> GetByCodeAsync(string code)
        {
            return await _context.medicineCategories
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Code == code);
        }
    }
}
