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

        public async Task<IEnumerable<Medicine>> GetAllAdvancedAsync()
        {
            return await _context.medicines
                .Include(m => m.MedicineCategory)
                .ToListAsync();
        }

        public async Task<Medicine?> GetByIdAdvancedAsync(Guid id)
        {
            return await _context.medicines
                .Include(m => m.MedicineCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
        }
    }
}
