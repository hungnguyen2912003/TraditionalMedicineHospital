using Project.Areas.Admin.Models.Entities;
using Project.Repositories;

namespace Project.Services.Implementations
{
    public class RoomService : BaseService<Room>, IBaseService<Room>
    {
        public RoomService(IBaseRepository<Room> repository) : base(repository)
        {
        }
    }
}
