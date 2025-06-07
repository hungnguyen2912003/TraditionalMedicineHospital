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
                .Include(p => p.TreatmentRecord)
                    .ThenInclude(tr => tr.Patient)
                        .ThenInclude(p => p.HealthInsurance)
                .Include(p => p.TreatmentRecord)
                    .ThenInclude(tr => tr.Prescriptions)
                        .ThenInclude(pre => pre.PrescriptionDetails)
                            .ThenInclude(pd => pd.Medicine)
                .Include(p => p.TreatmentRecord)
                    .ThenInclude(tr => tr.TreatmentRecordDetails)
                        .ThenInclude(trd => trd.Room)
                            .ThenInclude(r => r.TreatmentMethod)
                .Include(p => p.TreatmentRecord)
                    .ThenInclude(tr => tr.TreatmentRecordDetails)
                        .ThenInclude(trd => trd.TreatmentTrackings)
                .AsNoTracking()
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
                .AsSplitQuery()
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
                .AsSplitQuery()
                .FirstOrDefaultAsync(x => x.TreatmentRecordId == treatmentRecordId);
        }

        public async Task<IEnumerable<Payment>> GetAllForListViewAsync()
        {
            return await _context.payments
                .Select(p => new Payment
                {
                    Id = p.Id,
                    Code = p.Code,
                    PaymentDate = p.PaymentDate,
                    Status = p.Status,
                    TreatmentRecord = new TreatmentRecord
                    {
                        Id = p.TreatmentRecord.Id,
                        Code = p.TreatmentRecord.Code,
                        Patient = new Patient
                        {
                            Id = p.TreatmentRecord.Patient.Id,
                            Name = p.TreatmentRecord.Patient.Name,
                            HealthInsurance = p.TreatmentRecord.Patient.HealthInsurance != null ? new HealthInsurance
                            {
                                Number = p.TreatmentRecord.Patient.HealthInsurance.Number,
                                ExpiryDate = p.TreatmentRecord.Patient.HealthInsurance.ExpiryDate,
                                PlaceOfRegistration = p.TreatmentRecord.Patient.HealthInsurance.PlaceOfRegistration,
                                IsRightRoute = p.TreatmentRecord.Patient.HealthInsurance.IsRightRoute
                            } : null
                        },
                        AdvancePayment = p.TreatmentRecord.AdvancePayment,
                        Prescriptions = p.TreatmentRecord.Prescriptions.Select(pre => new Prescription
                        {
                            Id = pre.Id,
                            PrescriptionDetails = pre.PrescriptionDetails.Select(pd => new PrescriptionDetail
                            {
                                Quantity = pd.Quantity,
                                Medicine = pd.Medicine != null ? new Medicine
                                {
                                    Id = pd.Medicine.Id,
                                    Name = pd.Medicine.Name,
                                    Price = pd.Medicine.Price,
                                    MedicineCategory = pd.Medicine.MedicineCategory != null ? new MedicineCategory
                                    {
                                        Id = pd.Medicine.MedicineCategory.Id,
                                        Name = pd.Medicine.MedicineCategory.Name
                                    } : null!
                                } : null
                            }).ToList()
                        }).ToList(),
                        TreatmentRecordDetails = p.TreatmentRecord.TreatmentRecordDetails.Select(trd => new TreatmentRecordDetail
                        {
                            Id = trd.Id,
                            Room = trd.Room != null ? new Room
                            {
                                Id = trd.Room.Id,
                                Name = trd.Room.Name,
                                Department = trd.Room.Department != null ? new Department
                                {
                                    Id = trd.Room.Department.Id,
                                    Name = trd.Room.Department.Name
                                } : null!,
                                TreatmentMethod = trd.Room.TreatmentMethod != null ? new TreatmentMethod
                                {
                                    Id = trd.Room.TreatmentMethod.Id,
                                    Name = trd.Room.TreatmentMethod.Name,
                                    Cost = trd.Room.TreatmentMethod.Cost
                                } : null
                            } : null!,
                            TreatmentTrackings = trd.TreatmentTrackings.Select(tt => new TreatmentTracking
                            {
                                Id = tt.Id,
                                Status = tt.Status
                            }).ToList()
                        }).ToList()
                    }
                })
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
