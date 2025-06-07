using AutoMapper;
using Project.Areas.Admin.Models.Entities;
using Project.Areas.BacSi.Models.DTOs;
using Project.Areas.BacSi.Models.ViewModels;

namespace Project.Mappers
{
    public class TreatmentRecordProfile : Profile
    {
        public TreatmentRecordProfile()
        {
            // Treatment record
            CreateMap<TreatmentRecordDto, TreatmentRecord>()
                .ForMember(dest => dest.PatientId, opt => opt.MapFrom(src => src.PatientId))
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(dest => dest.Diagnosis, opt => opt.MapFrom(src => src.Diagnosis))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.Note, opt => opt.MapFrom(src => src.Note))
                .ForMember(dest => dest.AdvancePayment, opt => opt.MapFrom(src => src.AdvancePayment));

            CreateMap<TreatmentRecord, TreatmentRecordDto>()
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(dest => dest.Diagnosis, opt => opt.MapFrom(src => src.Diagnosis))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.Note, opt => opt.MapFrom(src => src.Note))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.PatientId, opt => opt.MapFrom(src => src.PatientId))
                .ForMember(dest => dest.AdvancePayment, opt => opt.MapFrom(src => src.AdvancePayment));

            // Assignment
            CreateMap<AssignmentDto, Assignment>()
                .ForMember(dest => dest.EmployeeId, opt => opt.Ignore())
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.Note, opt => opt.MapFrom(src => src.Note));

            CreateMap<Assignment, AssignmentDto>()
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(dest => dest.EmployeeId, opt => opt.MapFrom(src => src.EmployeeId))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.Note, opt => opt.MapFrom(src => src.Note))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy));

            // Treatment record detail
            CreateMap<TreatmentRecordDetail, TreatmentRecordDetailDto>()
                .ForMember(dest => dest.TreatmentMethodName, opt => opt.MapFrom(src => src.Room.TreatmentMethod!.Name))
                .ForMember(dest => dest.RoomName, opt => opt.MapFrom(src => src.Room.Name))
                .ForMember(dest => dest.TreatmentMethodId, opt => opt.MapFrom(src => src.Room.TreatmentMethodId));

            CreateMap<TreatmentRecordDetailDto, TreatmentRecordDetail>()
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(dest => dest.RoomId, opt => opt.MapFrom(src => src.RoomId))
                .ForMember(dest => dest.Note, opt => opt.MapFrom(src => src.Note))
                .ForMember(dest => dest.TreatmentRecordId, opt => opt.MapFrom(src => src.TreatmentRecordId));

            // Treatment record regulation
            CreateMap<TreatmentRecord_Regulation, RegulationDto>()
                .ForMember(dest => dest.RegulationName, opt => opt.MapFrom(src => src.Regulation.Name));

            CreateMap<RegulationDto, TreatmentRecord_Regulation>()
                .ForMember(dest => dest.TreatmentRecordId, opt => opt.Ignore())
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(dest => dest.RegulationId, opt => opt.MapFrom(src => src.RegulationId))
                .ForMember(dest => dest.ExecutionDate, opt => opt.MapFrom(src => src.ExecutionDate))
                .ForMember(dest => dest.Note, opt => opt.MapFrom(src => src.Note));


            CreateMap<TreatmentRecord, TreatmentRecordViewModel>()
                .ForMember(dest => dest.PatientName,
                    opt => opt.MapFrom(src => src.Patient != null ? src.Patient.Name : "Không xác định"));
        }
    }
}
