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

            CreateMap<Patient, ReceptionPatientDto>()
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
                .ForMember(dest => dest.PlaceOfRegistration, opt => opt.MapFrom(src => src.HealthInsurancePlaceOfRegistration))
                .ForMember(dest => dest.IsRightRoute, opt => opt.MapFrom(src => src.HealthInsuranceIsRightRoute));

            // Treatment record
            CreateMap<ReceptionTreatmentRecordDto, TreatmentRecord>()
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(dest => dest.Diagnosis, opt => opt.MapFrom(src => src.Diagnosis))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.Note, opt => opt.MapFrom(src => src.Note))
                .ForMember(dest => dest.AdvancePayment, opt => opt.MapFrom(src => src.AdvancePayment));

            CreateMap<TreatmentRecord, ReceptionTreatmentRecordDto>()
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(dest => dest.Diagnosis, opt => opt.MapFrom(src => src.Diagnosis))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.Note, opt => opt.MapFrom(src => src.Note))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.PatientId, opt => opt.MapFrom(src => src.PatientId))
                .ForMember(dest => dest.AdvancePayment, opt => opt.MapFrom(src => src.AdvancePayment));

            // Assignment
            CreateMap<ReceptionAssignmentDto, Assignment>()
                .ForMember(dest => dest.EmployeeId, opt => opt.Ignore())
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.Note, opt => opt.MapFrom(src => src.Note));

            CreateMap<Assignment, ReceptionAssignmentDto>()
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(dest => dest.EmployeeId, opt => opt.MapFrom(src => src.EmployeeId))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.Note, opt => opt.MapFrom(src => src.Note))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy));

            // Treatment record detail
            CreateMap<TreatmentRecordDetail, ReceptionTreatmentRecordDetailDto>()
                .ForMember(dest => dest.TreatmentMethodName, opt => opt.MapFrom(src => src.Room.TreatmentMethod!.Name))
                .ForMember(dest => dest.RoomName, opt => opt.MapFrom(src => src.Room.Name))
                .ForMember(dest => dest.TreatmentMethodId, opt => opt.MapFrom(src => src.Room.TreatmentMethodId));

            CreateMap<ReceptionTreatmentRecordDetailDto, TreatmentRecordDetail>()
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(dest => dest.RoomId, opt => opt.MapFrom(src => src.RoomId))
                .ForMember(dest => dest.Note, opt => opt.MapFrom(src => src.Note));

            // Treatment record regulation
            CreateMap<TreatmentRecord_Regulation, ReceptionTreatmentRecordRegulationDto>()
                .ForMember(dest => dest.RegulationName, opt => opt.MapFrom(src => src.Regulation.Name));

            CreateMap<ReceptionTreatmentRecordRegulationDto, TreatmentRecord_Regulation>()
                .ForMember(dest => dest.TreatmentRecordId, opt => opt.Ignore())
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(dest => dest.RegulationId, opt => opt.MapFrom(src => src.RegulationId))
                .ForMember(dest => dest.ExecutionDate, opt => opt.MapFrom(src => src.ExecutionDate))
                .ForMember(dest => dest.Note, opt => opt.MapFrom(src => src.Note));
        }
    }
}
