using Project.Areas.Admin.Models.Entities;

namespace Project.Repositories.Interfaces
{
    public interface IRoomRepository : IBaseRepository<Room>
    {
        Task<IEnumerable<Room>> GetAllAdvancedAsync();
        Task<Room?> GetByIdAdvancedAsync(Guid id);
    }
}
