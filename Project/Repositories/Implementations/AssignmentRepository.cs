using Microsoft.EntityFrameworkCore;
using Project.Areas.Staff.Models.Entities;
using Project.Datas;
using Project.Repositories.Interfaces;

namespace Project.Repositories.Implementations
{
    public class AssignmentRepository : BaseRepository<Assignment>, IAssignmentRepository
    {
        public AssignmentRepository(TraditionalMedicineHospitalDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Assignment>> GetAllAdvancedAsync()
        {
            return await _context.assignments
                .Include(a => a.Employee)
                .Include(a => a.TreatmentRecord)
                .Where(a => a.IsActive)
                .ToListAsync();
        }

        public async Task<Assignment?> GetByIdAdvancedAsync(Guid id)
        {
            return await _context.assignments
                .Include(a => a.Employee)
                .Include(a => a.TreatmentRecord)
                .FirstOrDefaultAsync(a => a.Id == id && a.IsActive);
        }

        public async Task<List<Assignment>> GetByTreatmentRecordIdAsync(Guid treatmentRecordId)
        {
            return await _context.assignments
                .Include(a => a.Employee)
                .Where(a => a.TreatmentRecordId == treatmentRecordId && a.IsActive)
                .ToListAsync();
        }
    }
}

