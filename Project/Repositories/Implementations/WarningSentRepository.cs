using Project.Areas.Admin.Models.Entities;
using Project.Datas;
using Project.Repositories.Interfaces;

namespace Project.Repositories.Implementations
{
    public class WarningSentRepository : BaseRepository<WarningSent>, IWarningSentRepository
    {
        public WarningSentRepository(TraditionalMedicineHospitalDbContext context) : base(context) { }
    }
}