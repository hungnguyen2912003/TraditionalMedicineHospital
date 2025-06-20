using Project.Areas.Admin.Models.Entities;

namespace Project.Repositories.Interfaces
{
    public interface ITreatmentRecordRegulationRepository : IBaseRepository<TreatmentRecord_Regulation>
    {
        Task<List<TreatmentRecord_Regulation>> GetByTreatmentRecordIdAsync(Guid treatmentRecordId);
    }
}

