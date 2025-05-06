using Project.Areas.Staff.Models.Entities;
using Project.Areas.Staff.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.Repositories.Interfaces
{
    public interface ITreatmentRecordDetailRepository : IBaseRepository<TreatmentRecordDetail>
    {
        Task<List<TreatmentRecordDetail>> GetByTreatmentRecordIdAsync(Guid treatmentRecordId);
        new Task<TreatmentRecordDetail?> GetByCodeAsync(string code);
        Task<TreatmentRecordDetailDto?> GetDetailWithNamesAsync(string code);
        Task<List<Patient>> GetPatientsByRoomIdAsync(Guid roomId);
        Task<TreatmentRecordDetail?> GetByPatientAndRoomAsync(Guid patientId, Guid roomId);
    }
}

