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
                .Include(m => m.Room)
                    .ThenInclude(m => m.Department)
                .Include(m => m.Room)
                    .ThenInclude(m => m.TreatmentMethod)
                .ToListAsync();
        }

        public async Task<Employee?> GetByIdAdvancedAsync(Guid id)
        {
            return await _context.employees
                .Include(m => m.EmployeeCategory)
                .Include(m => m.Room)
                    .ThenInclude(m => m.Department)
                .Include(m => m.Room)
                    .ThenInclude(m => m.TreatmentMethod)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Employee?> GetByUsernameAsync(string username)
        {
            return await _context.employees.FirstOrDefaultAsync(e => e.User!.Username == username);
        }

        public async Task<IEnumerable<Employee>> GetByCodesAsync(IEnumerable<string> codes)
        {
            return await _context.employees.Where(e => codes.Contains(e.Code)).ToListAsync();
        }
    }
}
