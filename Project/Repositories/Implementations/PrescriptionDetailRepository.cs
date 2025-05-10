using Microsoft.EntityFrameworkCore;
using Project.Areas.Staff.Models.Entities;
using Project.Datas;
using Project.Repositories;
using Repositories.Interfaces;

namespace Repositories.Implementations
{
    public class PrescriptionDetailRepository : BaseRepository<PrescriptionDetail>, IPrescriptionDetailRepository
    {
        public PrescriptionDetailRepository(TraditionalMedicineHospitalDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<PrescriptionDetail>> GetAllAdvancedAsync()
        {
            return await _context.prescriptionDetails
                .Include(p => p.Prescription)
                .Include(p => p.Medicine)
                .ToListAsync();
        }


        public async Task<PrescriptionDetail?> GetByIdAdvancedAsync(Guid id)
        {
            return await _context.prescriptionDetails
                .Include(p => p.Prescription)
                .Include(p => p.Medicine)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<PrescriptionDetail>> GetByPrescriptionIdAsync(Guid prescriptionId)
        {
            return await _context.prescriptionDetails
                .Include(p => p.Medicine)
                .Where(p => p.PrescriptionId == prescriptionId)
                .ToListAsync();
        }
    }
}
