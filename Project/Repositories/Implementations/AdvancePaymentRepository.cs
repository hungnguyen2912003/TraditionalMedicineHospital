using Microsoft.EntityFrameworkCore;
using Project.Areas.Admin.Models.Entities;
using Project.Datas;
using Project.Repositories.Interfaces;

namespace Project.Repositories.Implementations
{
    public class AdvancePaymentRepository : BaseRepository<AdvancePayment>, IAdvancePaymentRepository
    {
        public AdvancePaymentRepository(TraditionalMedicineHospitalDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<AdvancePayment>> GetAllAdvancedAsync()
        {
            return await _context.advancePayments
                .Include(p => p.TreatmentRecord)
                    .ThenInclude(tr => tr.Patient)
                .ToListAsync();
        }

        public async Task<AdvancePayment?> GetByIdAdvancedAsync(Guid id)
        {
            return await _context.advancePayments
                .Include(p => p.TreatmentRecord)
                    .ThenInclude(tr => tr.Patient)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<AdvancePayment?> GetByTreatmentRecordIdAsync(Guid treatmentRecordId)
        {
            return await _context.advancePayments
                .Include(p => p.TreatmentRecord)
                    .ThenInclude(tr => tr.Patient)
                .FirstOrDefaultAsync(p => p.TreatmentRecordId == treatmentRecordId);
        }

        public async Task<IEnumerable<AdvancePayment>> GetListByTreatmentRecordIdAsync(Guid treatmentRecordId)
        {
            return await _context.advancePayments
                .Where(ap => ap.TreatmentRecordId == treatmentRecordId)
                .OrderBy(ap => ap.PaymentDate)
                .ToListAsync();
        }
    }
}
