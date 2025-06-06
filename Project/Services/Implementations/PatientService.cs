using Project.Areas.Admin.Models.Entities;
using Project.Repositories;

namespace Project.Services.Implementations
{
    public class PatientService : BaseService<Patient>, IBaseService<Patient>
    {
        public PatientService(IBaseRepository<Patient> repository) : base(repository)
        {
        }
    }
}
