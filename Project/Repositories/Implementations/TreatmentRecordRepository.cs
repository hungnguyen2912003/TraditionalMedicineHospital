using Microsoft.EntityFrameworkCore;
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
                .ToListAsync();
        }

        public async Task<TreatmentRecord?> GetByIdAdvancedAsync(Guid id)
        {
            return await _context.treatmentRecords
                .Include(m => m.Patient)
                .FirstOrDefaultAsync(m => m.Id == id);
        }
    }
}
