using Microsoft.EntityFrameworkCore;
using Project.Areas.Admin.Models.Entities;
using Project.Datas;
using Project.Repositories;
using Repositories.Interfaces;

namespace Repositories.Implementations
{
    public class PrescriptionRepository : BaseRepository<Prescription>, IPrescriptionRepository
    {
        public PrescriptionRepository(TraditionalMedicineHospitalDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Prescription>> GetAllAdvancedAsync()
        {
            return await _context.prescriptions
                .Include(p => p.PrescriptionDetails)
                    .ThenInclude(d => d.Medicine)
                .Include(p => p.TreatmentRecord)
                    .ThenInclude(t => t!.Patient)
                .Include(p => p.TreatmentRecord)
                    .ThenInclude(t => t!.Assignments)
                        .ThenInclude(a => a.Employee)
                .ToListAsync();
        }

        public async Task<Prescription?> GetByIdAdvancedAsync(Guid id)
        {
            return await _context.prescriptions
                .Include(p => p.PrescriptionDetails)
                    .ThenInclude(d => d.Medicine)
                .Include(p => p.TreatmentRecord)
                    .ThenInclude(t => t!.Patient)
                .Include(p => p.TreatmentRecord)
                    .ThenInclude(t => t!.Assignments)
                        .ThenInclude(a => a.Employee)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Prescription>> GetByTreatmentRecordIdAsync(Guid treatmentRecordId)
        {
            return await _context.prescriptions
                .Include(p => p.PrescriptionDetails)
                    .ThenInclude(d => d.Medicine)
                .Include(p => p.TreatmentRecord)
                .Where(p => p.TreatmentRecordId == treatmentRecordId)
                .ToListAsync();
        }
    }
}
