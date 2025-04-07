using Microsoft.EntityFrameworkCore;
using Project.Areas.Admin.Models.Entities;
using Project.Datas;
using Project.Repositories.Interfaces;

namespace Project.Repositories.Implementations
{
    public class TreatmentMethodRepository : BaseRepository<TreatmentMethod>, ITreatmentMethodRepository
    {
        public TreatmentMethodRepository(TraditionalMedicineHospitalDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<TreatmentMethod>> GetAllAdvancedAsync()
        {
            return await _context.treatments
                .Include(m => m.Department)
                .ToListAsync();
        }

        public async Task<TreatmentMethod?> GetByIdAdvancedAsync(Guid id)
        {
            return await _context.treatments
                .Include(m => m.Department)
                .FirstOrDefaultAsync(m => m.Id == id);
        }
    }
}
