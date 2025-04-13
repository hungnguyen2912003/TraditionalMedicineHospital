using AutoMapper;
using Project.Areas.Staff.Models.DTOs;
using Project.Areas.Staff.Models.Entities;
using Project.Models.Enums;

namespace Project.Mappers
{
    public class ReceptionProfile : Profile
    {
        public ReceptionProfile()
        {
            // Patient
            CreateMap<ReceptionPatientDto, Patient>()
                .ForMember(dest => dest.Images, opt => opt.Ignore());

            // HealthInsurance
            CreateMap<ReceptionPatientDto, HealthInsurance>()
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.HealthInsuranceCode))
                .ForMember(dest => dest.Number, opt => opt.MapFrom(src => src.HealthInsuranceNumber))
                .ForMember(dest => dest.ExpiryDate, opt => opt.MapFrom(src => src.HealthInsuranceExpiryDate))
                .ForMember(dest => dest.PlaceOfRegistration, opt => opt.MapFrom(src => src.HealthInsurancePlaceOfRegistration))
                .ForMember(dest => dest.PatientId, opt => opt.Ignore());

            // TreatmentRecord
            CreateMap<ReceptionTreatmentRecordDto, TreatmentRecord>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => TreatmentStatus.DangDieuTri))
                .ForMember(dest => dest.Note, opt => opt.Ignore())
                .ForMember(dest => dest.PatientId, opt => opt.Ignore());

            // TreatmentRecordDetail
            CreateMap<ReceptionTreatmentRecordDetailDto, TreatmentRecordDetail>()
                .ForMember(dest => dest.TreatmentRecordId, opt => opt.Ignore());

            // Assignment
            CreateMap<ReceptionAssignmentDto, Assignment>()
                .ForMember(dest => dest.TreatmentRecordId, opt => opt.Ignore());

            // TreatmentRecord_Regulation
            CreateMap<ReceptionTreatmentRecordRegulationDto, TreatmentRecord_Regulation>()
                .ForMember(dest => dest.TreatmentRecordId, opt => opt.Ignore());
        }
    }
}
