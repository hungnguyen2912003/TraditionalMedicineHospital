using AutoMapper;
using Project.Areas.Staff.Models.DTOs.ReceptionDTO;
using Project.Areas.Staff.Models.Entities;

namespace Project.Mappers
{
    public class ReceptionProfile : Profile
    {
        public ReceptionProfile()
        {
            // Patient
            CreateMap<ReceptionPatientDto, Patient>();

            // Health insurance
            CreateMap<ReceptionPatientDto, HealthInsurance>()
                .ForMember(dest => dest.PatientId, opt => opt.Ignore());

            // Treatment record
            CreateMap<ReceptionTreatmentRecordDto, TreatmentRecord>();

            // Assignment
            CreateMap<ReceptionAssignmentDto, Assignment>()
                .ForMember(dest => dest.EmployeeId, opt => opt.Ignore());

            // Treatment record detail
            CreateMap<ReceptionTreatmentRecordDetailDto, TreatmentRecordDetail>()
                .ForMember(dest => dest.RoomId, opt => opt.Ignore())
                .ForMember(dest => dest.TreatmentRecordId, opt => opt.Ignore());

            // Treatment record regulation
            CreateMap<ReceptionTreatmentRecordRegulationDto, TreatmentRecord_Regulation>()
                .ForMember(dest => dest.RegulationId, opt => opt.Ignore());
        }
    }
}
