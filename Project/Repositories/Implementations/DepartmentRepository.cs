using Microsoft.EntityFrameworkCore;
using Project.Areas.Admin.Models.Entities;
using Project.Datas;
using Project.Repositories.Interfaces;

namespace Project.Repositories.Implementations
{
    public class DepartmentRepository : BaseRepository<Department>, IDepartmentRepository
    {
        public DepartmentRepository(TraditionalMedicineHospitalDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Department>> GetAllAdvancedAsync()
        {
            return await _context.departments
                        .Include(r => r.Rooms)
                        .ToListAsync();
        }

        public async Task<Department?> GetByIdAdvancedAsync(Guid id)
        {
            return await _context.departments
                        .Include(r => r.Rooms)
                        .FirstOrDefaultAsync(r => r.Id == id);
        }
    }
}
