using Microsoft.EntityFrameworkCore;
using Project.Areas.Admin.Models.Entities;
using Project.Datas;
using Project.Repositories.Interfaces;

namespace Project.Repositories.Implementations
{
    public class TreatmentMethodRepository : BaseRepository<TreatmentMethod>, ITreatmentMethodRepository
    {
        public TreatmentMethodRepository(TraditionalMedicineHospitalDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<TreatmentMethod>> GetAllAdvancedAsync()
        {
            return await _context.treatments
                //.Include(t => t.Department)
                .ToListAsync();
        }

        public async Task<TreatmentMethod?> GetByIdAdvancedAsync(Guid id)
        {
            return await _context.treatments
                //.Include(t => t.Department)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<TreatmentMethod>> GetAvailableTreatmentMethodsAsync(Guid treatmentRecordId)
        {
            // Get the treatment record to find the employee's department
            var treatmentRecord = await _context.treatmentRecords
                .Include(tr => tr.Assignments)
                .ThenInclude(a => a.Employee)
                    .ThenInclude(r => r.Room)
                        .ThenInclude(r => r.Department)
                .FirstOrDefaultAsync(tr => tr.Id == treatmentRecordId);

            if (treatmentRecord == null || !treatmentRecord.Assignments.Any())
            {
                return Enumerable.Empty<TreatmentMethod>();
            }

            // Get the department of the assigned employee
            var departmentId = treatmentRecord.Assignments.First().Employee.Room.DepartmentId;

            // Get treatment methods that are already used in this treatment record
            var usedTreatmentMethodIds = await _context.treatmentRecordDetails
                .Where(trd => trd.TreatmentRecordId == treatmentRecordId)
                .Select(trd => trd.Room.TreatmentMethodId)
                .ToListAsync();

            // Return treatment methods that:
            // 1. Belong to the employee's department
            // 2. Haven't been used in this treatment record yet
            return await _context.treatments
                .Where(t => t.Rooms.Any(r => r.DepartmentId == departmentId) && !usedTreatmentMethodIds.Contains(t.Id))
                .ToListAsync();
        }

        public async Task<IEnumerable<TreatmentMethod>> GetAllByDepartmentAsync(Guid departmentId)
        {
            return await _context.treatments
                .Where(t => t.Rooms.Any(r => r.DepartmentId == departmentId))
                .ToListAsync();
        }
    }
}
