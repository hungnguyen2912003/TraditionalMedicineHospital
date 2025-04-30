using AutoMapper;
using Project.Areas.Staff.Models.DTOs;
using Project.Areas.Staff.Models.Entities;
using Project.Areas.Staff.Models.ViewModels;

namespace Project.Mappers
{
    public class HealthInsuranceProfile : Profile
    {
        public HealthInsuranceProfile()
        {
            CreateMap<HealthInsurance, HealthInsuranceDto>();
            CreateMap<HealthInsuranceDto, HealthInsurance>()
                .ForMember(dest => dest.IsActive, opt => opt.Ignore());
            CreateMap<HealthInsurance, HealthInsuranceViewModel>()
                .ForMember(dest => dest.PatientName,
                    opt => opt.MapFrom(src => src.Patient != null ? src.Patient.Name : "Không xác định"))
                .ForMember(dest => dest.IsRightRoute, opt => opt.MapFrom(src => src.IsRightRoute));
        }
    }
}
