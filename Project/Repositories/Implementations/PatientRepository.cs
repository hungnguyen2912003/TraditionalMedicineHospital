using Microsoft.EntityFrameworkCore;
using Project.Areas.Admin.Models.Entities;
using Project.Datas;
using Project.Repositories.Interfaces;

namespace Project.Repositories.Implementations
{
    public class PatientRepository : BaseRepository<Patient>, IPatientRepository
    {
        public PatientRepository(TraditionalMedicineHospitalDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Patient>> GetAllAdvancedAsync()
        {
            return await _context.patients
                .Include(m => m.HealthInsurance)
                .Include(m => m.TreatmentRecords)
                .ToListAsync();
        }

        public async Task<Patient?> GetByIdAdvancedAsync(Guid id)
        {
            return await _context.patients
                .Include(m => m.HealthInsurance)
                .Include(m => m.TreatmentRecords)
                .FirstOrDefaultAsync(m => m.Id == id);
        }
    }
}
