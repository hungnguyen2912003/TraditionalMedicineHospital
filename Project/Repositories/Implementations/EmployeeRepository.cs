using Microsoft.EntityFrameworkCore;
using Project.Areas.Admin.Data;
using Project.Areas.Admin.Models.Entities;
using Project.Repositories.Interfaces;

namespace Project.Repositories.Implementations
{
    public class EmployeeRepository : BaseRepository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(TraditionalMedicineHospitalDbContext context) : base(context)
        {
        }

        public async Task<bool> IsIdentityCardExistsAsync(string identityCard)
        {
            return await _context.employees.AnyAsync(e => e.IdentityCard == identityCard);
        }

        public async Task<IEnumerable<Employee>> GetAllWithCategoryAsync()
        {
            return await _context.employees
                .Include(m => m.EmployeeCategory)
                .ToListAsync();
        }

        public async Task<Employee?> GetByCodeAsync(string code)
        {
            return await _context.employees
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Code == code);
        }

        public async Task<Employee?> GetByIdWithCategoryAsync(Guid id)
        {
            return await _context.employees
                .Include(m => m.EmployeeCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Employee?> GetByNameAsync(string name)
        {
            return await _context.employees
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.FullName == name);
        }

        public async Task<bool> IsCodeExistsAsync(string code)
        {
            return await _context.employees.AnyAsync(e => e.Code == code);
        }
    }
}
