using Microsoft.EntityFrameworkCore;
using Project.Areas.Admin.Models.Entities;
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

        public async Task<HealthInsurance?> GetByPatientIdAsync(Guid patientId)
        {
            return await _context.healthInsurances
                .Include(m => m.Patient)
                .FirstOrDefaultAsync(m => m.PatientId == patientId);
        }
    }
}
