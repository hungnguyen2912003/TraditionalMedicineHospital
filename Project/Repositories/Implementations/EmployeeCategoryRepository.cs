using Microsoft.EntityFrameworkCore;
using Project.Areas.Admin.Models.Entities;
using Project.Datas;
using Project.Repositories.Interfaces;

namespace Project.Repositories.Implementations
{
    public class EmployeeCategoryRepository : BaseRepository<EmployeeCategory>, IEmployeeCategoryRepository
    {
        public EmployeeCategoryRepository(TraditionalMedicineHospitalDbContext context) : base(context)
        {
        }
    }
}
