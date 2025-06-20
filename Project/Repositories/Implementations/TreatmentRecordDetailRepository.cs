using Microsoft.EntityFrameworkCore;
using Project.Areas.Admin.Models.Entities;
using Project.Areas.BacSi.Models.DTOs;
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
                    .ThenInclude(r => r.Department)
                .Include(t => t.Room)
                    .ThenInclude(r => r.TreatmentMethod)
                .Include(t => t.TreatmentRecord)
                    .ThenInclude(t => t.Assignments)
                        .ThenInclude(a => a.Employee)
                .Where(t => t.TreatmentRecordId == treatmentRecordId)
                .ToListAsync();
        }

        public new async Task<TreatmentRecordDetail?> GetByCodeAsync(string code)
        {
            if (string.IsNullOrEmpty(code))
                return null;

            return await _context.treatmentRecordDetails
                .Include(x => x.Room)
                    .ThenInclude(x => x.TreatmentMethod)
                .Include(x => x.Room)
                    .ThenInclude(x => x.Department)
                .Include(x => x.TreatmentRecord)
                    .ThenInclude(x => x.Assignments)
                        .ThenInclude(x => x.Employee)
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

            // Lấy tên bác sĩ thực hiện từ CreatedBy (username)
            string doctorName = detail.CreatedBy;
            Guid employeeId = Guid.Empty;
            if (!string.IsNullOrEmpty(detail.CreatedBy))
            {
                var user = await _context.users
                    .Include(u => u.Employee)
                    .FirstOrDefaultAsync(u => u.Username == detail.CreatedBy);
                if (user?.Employee != null)
                {
                    doctorName = user.Employee.Name;
                    employeeId = user.Employee.Id;
                }
            }


            return new TreatmentRecordDetailDto
            {
                Code = detail.Code ?? string.Empty,
                RoomId = detail.RoomId,
                RoomName = detail.Room?.Name ?? string.Empty,
                TreatmentMethodId = detail.Room?.TreatmentMethodId ?? Guid.Empty,
                TreatmentMethodName = detail.Room?.TreatmentMethod?.Name ?? string.Empty,
                DoctorName = doctorName ?? string.Empty,
                Note = detail.Note ?? string.Empty,
                EmployeeId = employeeId,
                TreatmentRecordId = detail.TreatmentRecordId
            };
        }

        public async Task<List<Patient>> GetPatientsByRoomIdAsync(Guid roomId)
        {
            return await _context.treatmentRecordDetails
                .Where(d => d.RoomId == roomId)
                .Select(d => d.TreatmentRecord.Patient)
                .Distinct()
                .ToListAsync();
        }

        public async Task<TreatmentRecordDetail?> GetByPatientAndRoomAsync(Guid patientId, Guid roomId)
        {
            return await _context.treatmentRecordDetails
                .Include(d => d.TreatmentRecord)
                .Where(d => d.TreatmentRecord.PatientId == patientId && d.RoomId == roomId)
                .OrderByDescending(d => d.CreatedDate)
                .FirstOrDefaultAsync();
        }

        public async Task<TreatmentRecordDetail?> GetByIdAdvancedAsync(Guid id)
        {
            return await _context.treatmentRecordDetails
                .Include(x => x.Room)
                    .ThenInclude(x => x.TreatmentMethod)
                .Include(x => x.Room)
                    .ThenInclude(x => x.Department)
                .Include(x => x.TreatmentRecord)
                    .ThenInclude(x => x.Assignments)
                        .ThenInclude(x => x.Employee)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<TreatmentRecordDetail>> GetDetailsByRoomIdAsync(Guid roomId)
        {
            return await _context.treatmentRecordDetails
                .Include(d => d.TreatmentRecord)
                    .ThenInclude(p => p.Patient)
                .Include(d => d.Room)
                    .ThenInclude(r => r.TreatmentMethod)
                .Where(d => d.RoomId == roomId)
                .ToListAsync();
        }
    }
}

