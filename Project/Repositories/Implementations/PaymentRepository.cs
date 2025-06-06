using Microsoft.EntityFrameworkCore;
using Project.Areas.Admin.Models.Entities;
using Project.Datas;
using Project.Repositories.Interfaces;

namespace Project.Repositories.Implementations
{
    public class PaymentRepository : BaseRepository<Payment>, IPaymentRepository
    {
        public PaymentRepository(TraditionalMedicineHospitalDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Payment>> GetAllAdvancedAsync()
        {
            return await _context.payments
                .Include(tr => tr.TreatmentRecord)
                    .ThenInclude(p => p.Patient)
                        .ThenInclude(hi => hi.HealthInsurance)
                .Include(tr => tr.TreatmentRecord)
                    .ThenInclude(pre => pre.Prescriptions)
                        .ThenInclude(preDetail => preDetail.PrescriptionDetails)
                            .ThenInclude(m => m.Medicine)
                .Include(tr => tr.TreatmentRecord)
                    .ThenInclude(tr => tr.TreatmentRecordDetails)
                        .ThenInclude(r => r.Room)
                            .ThenInclude(tm => tm.TreatmentMethod)
                .Include(tr => tr.TreatmentRecord)
                    .ThenInclude(tr => tr.TreatmentRecordDetails)
                        .ThenInclude(r => r.TreatmentTrackings)
                .ToListAsync();
        }

        public async Task<Payment?> GetByIdAdvancedAsync(Guid id)
        {
            return await _context.payments
                .Include(tr => tr.TreatmentRecord)
                    .ThenInclude(p => p.Patient)
                        .ThenInclude(hi => hi.HealthInsurance)
                .Include(tr => tr.TreatmentRecord)
                    .ThenInclude(pre => pre.Prescriptions)
                        .ThenInclude(preDetail => preDetail.PrescriptionDetails)
                            .ThenInclude(m => m.Medicine)
                .Include(tr => tr.TreatmentRecord)
                    .ThenInclude(tr => tr.TreatmentRecordDetails)
                        .ThenInclude(r => r.Room)
                            .ThenInclude(tm => tm.TreatmentMethod)
                .Include(tr => tr.TreatmentRecord)
                    .ThenInclude(tr => tr.TreatmentRecordDetails)
                        .ThenInclude(r => r.TreatmentTrackings)
                .Include(tr => tr.TreatmentRecord)
                    .ThenInclude(tr => tr.TreatmentRecordDetails)
                        .ThenInclude(r => r.Room)
                            .ThenInclude(r => r.Department)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Payment?> GetByTreatmentRecordIdAsync(Guid treatmentRecordId)
        {
            return await _context.payments
                .Include(tr => tr.TreatmentRecord)
                    .ThenInclude(p => p.Patient)
                        .ThenInclude(hi => hi.HealthInsurance)
                .Include(tr => tr.TreatmentRecord)
                    .ThenInclude(pre => pre.Prescriptions)
                        .ThenInclude(preDetail => preDetail.PrescriptionDetails)
                            .ThenInclude(m => m.Medicine)
                .Include(tr => tr.TreatmentRecord)
                    .ThenInclude(tr => tr.TreatmentRecordDetails)
                        .ThenInclude(r => r.Room)
                            .ThenInclude(tm => tm.TreatmentMethod)
                .Include(tr => tr.TreatmentRecord)
                    .ThenInclude(tr => tr.TreatmentRecordDetails)
                        .ThenInclude(r => r.TreatmentTrackings)
                .Include(tr => tr.TreatmentRecord)
                    .ThenInclude(tr => tr.TreatmentRecordDetails)
                        .ThenInclude(r => r.Room)
                            .ThenInclude(r => r.Department)
                .FirstOrDefaultAsync(x => x.TreatmentRecordId == treatmentRecordId);
        }
    }
}
