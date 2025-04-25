using Microsoft.EntityFrameworkCore;
using Project.Areas.Staff.Models.DTOs;
using Project.Areas.Staff.Models.Entities;
using Project.Datas;
using Project.Repositories.Interfaces;

namespace Project.Repositories.Implementations
{
    public class TreatmentRecordDetailRepository : BaseRepository<TreatmentRecordDetail>, ITreatmentRecordDetailRepository
    {
        public TreatmentRecordDetailRepository(TraditionalMedicineHospitalDbContext context) : base(context)
        {
        }

        public async Task<List<TreatmentRecordDetail>> GetByTreatmentRecordIdAsync(Guid treatmentRecordId)
        {
            return await _context.treatmentRecordDetails
                .Include(t => t.Room)
                    .ThenInclude(r => r.TreatmentMethod)
                .Where(t => t.TreatmentRecordId == treatmentRecordId && t.IsActive)
                .ToListAsync();
        }

        public new async Task<TreatmentRecordDetail?> GetByCodeAsync(string code)
        {
            if (string.IsNullOrEmpty(code))
                return null;

            return await _context.treatmentRecordDetails
                .Include(x => x.Room)
                .ThenInclude(x => x.TreatmentMethod)
                .FirstOrDefaultAsync(x => x.Code == code);
        }

        public async Task<TreatmentRecordDetailDto?> GetDetailWithNamesAsync(string code)
        {
            if (string.IsNullOrEmpty(code))
                return null;

            var detail = await _context.treatmentRecordDetails
                .Include(x => x.Room)
                    .ThenInclude(x => x.TreatmentMethod)
                .Include(x => x.TreatmentRecord)
                    .ThenInclude(x => x.Assignments)
                        .ThenInclude(x => x.Employee)
                .FirstOrDefaultAsync(x => x.Code == code);

            if (detail == null)
                return null;

            // Get the doctor (employee) from the first active assignment
            var doctor = detail.TreatmentRecord.Assignments
                .Where(a => a.IsActive)
                .Select(a => a.Employee)
                .FirstOrDefault();

            return new TreatmentRecordDetailDto
            {
                Code = detail.Code ?? string.Empty,
                RoomId = detail.RoomId,
                RoomName = detail.Room?.Name ?? string.Empty,
                TreatmentMethodId = detail.Room?.TreatmentMethodId ?? Guid.Empty,
                TreatmentMethodName = detail.Room?.TreatmentMethod?.Name ?? string.Empty,
                DoctorName = doctor?.Name ?? string.Empty,
                Note = detail.Note ?? string.Empty
            };
        }

        public new async Task UpdateAsync(TreatmentRecordDetail detail)
        {
            if (detail == null)
                return;

            var existingDetail = await _context.treatmentRecordDetails
                .FirstOrDefaultAsync(x => x.Id == detail.Id);
                
            if (existingDetail != null)
            {
                existingDetail.RoomId = detail.RoomId;
                existingDetail.Note = detail.Note ?? string.Empty;
                await _context.SaveChangesAsync();
            }
        }
    }
}

