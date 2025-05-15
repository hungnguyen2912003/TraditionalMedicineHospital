using Microsoft.EntityFrameworkCore;
using Project.Areas.Admin.Models.Entities;
using Project.Areas.Staff.Models.Entities;
using Project.Datas;
using Project.Repositories.Interfaces;

namespace Project.Repositories.Implementations
{
    public class TreatmentRecordRepository : BaseRepository<TreatmentRecord>, ITreatmentRecordRepository
    {
        public TreatmentRecordRepository(TraditionalMedicineHospitalDbContext context) : base(context)
        {
        }
        public async Task<IEnumerable<TreatmentRecord>> GetAllAdvancedAsync()
        {
            return await _context.treatmentRecords
                .Include(m => m.Patient)
                    .ThenInclude(p => p.HealthInsurance)
                .Include(m => m.Assignments)
                    .ThenInclude(a => a.Employee)
                        .ThenInclude(r => r.Room)
                            .ThenInclude(d => d.Department)
                .Include(m => m.TreatmentRecord_Regulations)
                    .ThenInclude(tr => tr.Regulation)
                .ToListAsync();
        }

        public async Task<TreatmentRecord?> GetByIdAdvancedAsync(Guid id)
        {
            return await _context.treatmentRecords
                .Include(m => m.Patient)
                    .ThenInclude(p => p.HealthInsurance)
                .Include(m => m.TreatmentRecordDetails)
                    .ThenInclude(d => d.Room)
                        .ThenInclude(tm => tm.TreatmentMethod)
                .Include(m => m.TreatmentRecordDetails)
                    .ThenInclude(d => d.TreatmentTrackings)
                .Include(m => m.Assignments)
                    .ThenInclude(a => a.Employee)
                .Include(m => m.TreatmentRecord_Regulations)
                    .ThenInclude(tr => tr.Regulation)
                .Include(p => p.Prescriptions)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<TreatmentRecord>> GetByPatientIdAsync(Guid patientId)
        {
            return await _context.treatmentRecords
                .Include(m => m.Patient)
                .Where(m => m.PatientId == patientId)
                .ToListAsync();
        }
    }
}
