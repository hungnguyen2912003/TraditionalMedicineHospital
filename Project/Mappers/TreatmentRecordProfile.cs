using AutoMapper;
using Project.Areas.Admin.Models.Entities;
using Project.Areas.Admin.Models.ViewModels;
using Project.Areas.Staff.Models.DTOs;
using Project.Areas.Staff.Models.Entities;
using Project.Areas.Staff.Models.ViewModels;

namespace Project.Mappers
{
    public class TreatmentRecordProfile : Profile
    {
        public TreatmentRecordProfile()
        {
            CreateMap<TreatmentRecord, TreatmentRecordDto>();
            CreateMap<TreatmentRecordDto, TreatmentRecord>();
            CreateMap<TreatmentRecord, TreatmentRecordViewModel>()
                .ForMember(dest => dest.PatientName,
                    opt => opt.MapFrom(src => src.Patient != null ? src.Patient.Name : "Không xác định"));
        }
    }
}
