using Project.Areas.Staff.Models.Entities;
using Project.Datas;
using Project.Repositories.Interfaces;

namespace Project.Repositories.Implementations
{
    public class AssignmentRepository : BaseRepository<Assignment>, IAssignmentRepository
    {
        public AssignmentRepository(TraditionalMedicineHospitalDbContext context) : base(context)
        {
        }
    }
}

