using Microsoft.EntityFrameworkCore;
using Project.Areas.Staff.Models.Entities;
using Project.Datas;
using Project.Repositories.Interfaces;

namespace Project.Repositories.Implementations
{
    public class TreatmentRecordRegulationRepository : BaseRepository<TreatmentRecord_Regulation>, ITreatmentRecordRegulationRepository
    {
        public TreatmentRecordRegulationRepository(TraditionalMedicineHospitalDbContext context) : base(context)
        {
        }

        public async Task<List<TreatmentRecord_Regulation>> GetByTreatmentRecordIdAsync(Guid treatmentRecordId)
        {
            return await _context.treatmentRecord_Regulations
                .Include(t => t.Regulation)
                .Where(t => t.TreatmentRecordId == treatmentRecordId)
                .ToListAsync();
        }
    }
}

