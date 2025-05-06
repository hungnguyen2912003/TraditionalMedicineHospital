using Microsoft.EntityFrameworkCore;
using Project.Areas.Staff.Models.DTOs.TrackingDTO;
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
                .Include(t => t.TreatmentRecordDetails)
                    .ThenInclude(d => d.TreatmentRecord)
                        .ThenInclude(r => r.Patient)
                .ToListAsync();
        }

    }
}