using Project.Areas.Admin.Data;
using Project.Areas.Admin.Models.Entities;
using Project.Repositories.Interfaces;

namespace Project.Repositories.Implementations
{
    public class DepartmentRepository : BaseRepository<Department>, IDepartmentRepository
    {
        public DepartmentRepository(TraditionalMedicineHospitalDbContext context) : base(context)
        {
        }
    }
}
