using Microsoft.EntityFrameworkCore;
using Project.Areas.Staff.Models.Entities;
using Project.Datas;
using Project.Repositories.Interfaces;

namespace Project.Repositories.Implementations
{
    public class TreatmentRecordDetailRepository : BaseRepository<TreatmentRecordDetail>, ITreatmentRecordDetailRepository
    {
        public TreatmentRecordDetailRepository(TraditionalMedicineHospitalDbContext context) : base(context)
        {
        }

        public async Task<List<TreatmentRecordDetail>> GetByTreatmentRecordIdAsync(Guid treatmentRecordId)
        {
            return await _context.treatmentRecordDetails
                .Include(t => t.Room)
                    .ThenInclude(r => r.TreatmentMethod)
                .Where(t => t.TreatmentRecordId == treatmentRecordId && t.IsActive)
                .ToListAsync();
        }
    }
}

