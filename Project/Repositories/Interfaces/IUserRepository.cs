using Project.Areas.Admin.Models.Entities;

namespace Project.Repositories.Interfaces
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<User?> GetEmailUserAsync(string email);
        Task<User?> GetByUsernameAsync(string username);
    }
}
