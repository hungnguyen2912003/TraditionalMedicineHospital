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
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public new async Task<User?> GetByCodeAsync(string code)
        {
            return await _context.users
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.Employee != null && u.Employee.Code == code);
        }

        public async Task<User?> GetByIdentifierAsync(string identifier)
        {
            return await _context.users
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u =>
                    (u.Employee != null && u.Employee.Code == identifier) ||
                    (u.Employee != null && u.Employee.EmailAddress == identifier));
        }
    }
}
