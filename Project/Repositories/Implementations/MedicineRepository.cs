using Microsoft.EntityFrameworkCore;
using Project.Areas.Admin.Data;
using Project.Areas.Admin.Models.Entities;
using Project.Repositories.Interfaces;

namespace Project.Repositories.Implementations
{
    public class MedicineRepository : BaseRepository<Medicine>, IMedicineRepository
    {
        public MedicineRepository(TraditionalMedicineHospitalDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Medicine>> GetAllWithCategoryAsync()
        {
            return await _context.medicines
                .Include(m => m.MedicineCategory)
                .ToListAsync();
        }

        public async Task<Medicine?> GetByCodeAsync(string code)
        {
            return await _context.medicines
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Code == code);
        }

        public async Task<Medicine?> GetByNameAsync(string name)
        {
            return await _context.medicines
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Name == name);
        }
    }
}
