using Microsoft.EntityFrameworkCore;
using Project.Areas.Admin.Models.Entities;
using Project.Datas;
using Project.Repositories.Interfaces;

namespace Project.Repositories.Implementations
{
    public class TreatmentTrackingRepository : BaseRepository<TreatmentTracking>, ITreatmentTrackingRepository
    {
        public TreatmentTrackingRepository(TraditionalMedicineHospitalDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<TreatmentTracking>> GetAllAdvancedAsync()
        {
            return await _context.treatmentTrackings
                .Include(t => t.TreatmentRecordDetail!)
                    .ThenInclude(tr => tr.TreatmentRecord)
                        .ThenInclude(p => p.Patient)
                .Include(t => t.TreatmentRecordDetail!)
                    .ThenInclude(r => r.Room)
                        .ThenInclude(d => d.Department)
                .Include(t => t.TreatmentRecordDetail!)
                    .ThenInclude(r => r.Room)
                        .ThenInclude(t => t.TreatmentMethod)
                .AsSplitQuery()
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

        public async Task<IEnumerable<TreatmentTracking>> GetByCreatedByAsync(string createdBy)
        {
            return await _context.treatmentTrackings
                .Where(t => t.CreatedBy == createdBy)
                .Include(t => t.TreatmentRecordDetail!)
                    .ThenInclude(tr => tr.TreatmentRecord)
                        .ThenInclude(p => p.Patient)
                .Include(t => t.TreatmentRecordDetail!)
                    .ThenInclude(r => r.Room)
                        .ThenInclude(d => d.Department)
                .Include(t => t.TreatmentRecordDetail!)
                    .ThenInclude(r => r.Room)
                        .ThenInclude(t => t.TreatmentMethod)
                .AsSplitQuery()
                .OrderBy(t => t.TrackingDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<TreatmentTracking>> GetByDepartmentAsync(string departmentName)
        {
            return await _context.treatmentTrackings
                .Include(t => t.TreatmentRecordDetail!)
                    .ThenInclude(tr => tr.TreatmentRecord)
                        .ThenInclude(p => p.Patient)
                .Include(t => t.TreatmentRecordDetail!)
                    .ThenInclude(r => r.Room)
                        .ThenInclude(d => d.Department)
                .Include(t => t.TreatmentRecordDetail!)
                    .ThenInclude(r => r.Room)
                        .ThenInclude(t => t.TreatmentMethod)
                .AsSplitQuery()
                .Where(t => t.TreatmentRecordDetail!.Room!.Department!.Name == departmentName)
                .OrderBy(t => t.TrackingDate)
                .ToListAsync();
        }
    }
}