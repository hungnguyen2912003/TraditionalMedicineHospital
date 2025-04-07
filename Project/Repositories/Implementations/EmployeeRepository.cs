using Microsoft.EntityFrameworkCore;
using Project.Areas.Admin.Models.Entities;
using Project.Datas;
using Project.Repositories.Interfaces;

namespace Project.Repositories.Implementations
{
    public class EmployeeRepository : BaseRepository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(TraditionalMedicineHospitalDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Employee>> GetAllAdvancedAsync()
        {
            return await _context.employees
                .Include(m => m.EmployeeCategory)
                .Include(m => m.Department)
                .ToListAsync();
        }

        public async Task<Employee?> GetByIdAdvancedAsync(Guid id)
        {
            return await _context.employees
                .Include(m => m.EmployeeCategory)
                .Include (m => m.Department)
                .FirstOrDefaultAsync(m => m.Id == id);
        }
    }
}
