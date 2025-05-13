using Microsoft.EntityFrameworkCore;
using Project.Areas.Staff.Models.Entities;
using Project.Datas;
using Project.Repositories.Interfaces;

namespace Project.Repositories.Implementations
{
    public class TreatmentTrackingRepository : BaseRepository<TreatmentTracking>, ITreatmentTrackingRepository
    {
        public TreatmentTrackingRepository(TraditionalMedicineHospitalDbContext context) : base(context)
        {
        }

        public async Task<List<TreatmentTracking>> GetAllAdvancedAsync()
        {
            return await _context.treatmentTrackings
                .Include(t => t.TreatmentRecordDetail!)
                    .ThenInclude(tr => tr.TreatmentRecord)
                        .ThenInclude(p => p.Patient)
                .Include(t => t.TreatmentRecordDetail!)
                    .ThenInclude(r => r.Room)
                .OrderBy(t => t.TrackingDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<TreatmentTracking>> GetByDetailIdAsync(Guid detailId)
        {
            return await _context.treatmentTrackings
                .Where(t => t.TreatmentRecordDetailId == detailId)
                .ToListAsync();
        }

        public async Task<IEnumerable<TreatmentTracking>> GetByPatientIdAsync(Guid patientId)
        {
            return await _context.treatmentTrackings
                .Where(t => t.TreatmentRecordDetail!.TreatmentRecord!.PatientId == patientId)
                .ToListAsync();
        }
    }
}