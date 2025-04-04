using Project.Areas.Admin.Models.Entities;
using Project.Repositories;

namespace Project.Services.Implementations
{
    public class DepartmentService : BaseService<Department>, IBaseService<Department>
    {
        public DepartmentService(IBaseRepository<Department> repository)
                    : base(repository)
        {
        }
    }
}
