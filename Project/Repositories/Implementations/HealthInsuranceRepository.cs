using Microsoft.EntityFrameworkCore;
using Project.Areas.Staff.Models.Entities;
using Project.Datas;
using Project.Repositories.Interfaces;

namespace Project.Repositories.Implementations
{
    public class HealthInsuranceRepository : BaseRepository<HealthInsurance>, IHealthInsuranceRepository
    {
        public HealthInsuranceRepository(TraditionalMedicineHospitalDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<HealthInsurance>> GetAllAdvancedAsync()
        {
            return await _context.healthInsurances
                .Include(h => h.Patient)
                .ToListAsync();
        }

        public async Task<HealthInsurance?> GetByIdAdvancedAsync(Guid id)
        {
            return await _context.healthInsurances
                .Include(m => m.Patient)
                .FirstOrDefaultAsync(m => m.Id == id);
        }
    }
}
