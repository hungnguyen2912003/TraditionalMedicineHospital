using Project.Areas.Admin.Models.Entities;
using Project.Datas;
using Project.Repositories.Interfaces;

namespace Project.Repositories.Implementations
{
    public class WarningMailSentRepository : BaseRepository<WarningMailSent>, IWarningMailSentRepository
    {
        public WarningMailSentRepository(TraditionalMedicineHospitalDbContext context) : base(context) { }
    }
}