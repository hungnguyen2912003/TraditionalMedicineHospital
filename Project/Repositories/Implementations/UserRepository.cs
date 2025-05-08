using Microsoft.EntityFrameworkCore;
using Project.Areas.Admin.Models.Entities;
using Project.Datas;
using Project.Repositories.Interfaces;

namespace Project.Repositories.Implementations
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(TraditionalMedicineHospitalDbContext context) : base(context)
        {
        }

        public async Task<User?> GetEmailUserAsync(string email)
        {
            return await _context.users
                .FirstOrDefaultAsync(u => u.Employee != null && u.Employee.EmailAddress == email);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.users
                .Include(u => u.Employee!)
                    .ThenInclude(e => e.EmployeeCategory)
                .Include(u => u.Employee!)
                    .ThenInclude(e => e.Room!)
                        .ThenInclude(r => r.Department)
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public new async Task<User?> GetByCodeAsync(string code)
        {
            return await _context.users
                .Include(u => u.Employee!)
                    .ThenInclude(e => e.EmployeeCategory)
                .Include(u => u.Employee!)
                    .ThenInclude(e => e.Room!)
                        .ThenInclude(r => r.Department)
                .FirstOrDefaultAsync(u => u.Employee != null && u.Employee.Code == code);
        }

        public async Task<User?> GetByIdentifierAsync(string identifier)
        {
            return await _context.users
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u =>
                    u.Username == identifier ||
                    (u.Employee != null && u.Employee.Code == identifier) ||
                    (u.Employee != null && u.Employee.EmailAddress == identifier));
        }

        public async Task<Employee?> GetCurrentEmployee(Guid userId)
        {
            var user = await _context.users
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.Id == userId);
            return user?.Employee;
        }

        public async Task<IEnumerable<User>> GetAllAdvancedAsync()
        {
            return await _context.users
                .Include(u => u.Employee)
                .ToListAsync();
        }
    }
}
