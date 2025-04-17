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
            CreateMap<ReceptionPatientDto, Patient>()
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
                .ForMember(dest => dest.IdentityNumber, opt => opt.MapFrom(src => src.IdentityNumber))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.EmailAddress, opt => opt.MapFrom(src => src.EmailAddress));

            // Health insurance
            CreateMap<ReceptionPatientDto, HealthInsurance>()
                .ForMember(dest => dest.PatientId, opt => opt.Ignore())
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.HealthInsuranceCode))
                .ForMember(dest => dest.Number, opt => opt.MapFrom(src => src.HealthInsuranceNumber))
                .ForMember(dest => dest.ExpiryDate, opt => opt.MapFrom(src => src.HealthInsuranceExpiryDate))
                .ForMember(dest => dest.PlaceOfRegistration, opt => opt.MapFrom(src => src.HealthInsurancePlaceOfRegistration));

            // Treatment record
            CreateMap<ReceptionTreatmentRecordDto, TreatmentRecord>()
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(dest => dest.Diagnosis, opt => opt.MapFrom(src => src.Diagnosis))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.Note, opt => opt.MapFrom(src => src.Note));

            // Assignment
            CreateMap<ReceptionAssignmentDto, Assignment>()
                .ForMember(dest => dest.EmployeeId, opt => opt.Ignore())
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.Note, opt => opt.MapFrom(src => src.Note));

            // Treatment record detail
            CreateMap<ReceptionTreatmentRecordDetailDto, TreatmentRecordDetail>()
                .ForMember(dest => dest.TreatmentRecordId, opt => opt.Ignore())
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(dest => dest.RoomId, opt => opt.MapFrom(src => src.RoomId))
                .ForMember(dest => dest.Note, opt => opt.MapFrom(src => src.Note));

            // Treatment record regulation
            CreateMap<ReceptionTreatmentRecordRegulationDto, TreatmentRecord_Regulation>()
                .ForMember(dest => dest.TreatmentRecordId, opt => opt.Ignore())
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(dest => dest.RegulationId, opt => opt.MapFrom(src => src.RegulationId))
                .ForMember(dest => dest.ExecutionDate, opt => opt.MapFrom(src => src.ExecutionDate))
                .ForMember(dest => dest.Note, opt => opt.MapFrom(src => src.Note));
        }
    }
}
