using Project.Areas.Admin.Data;
using Project.Areas.Admin.Models.Entities;
using Project.Repositories.Interfaces;

namespace Project.Repositories.Implementations
{
    public class RegulationRepository : BaseRepository<Regulation>, IRegulationRepository
    {
        public RegulationRepository(TraditionalMedicineHospitalDbContext context) : base(context)
        {
        }
    }
}
