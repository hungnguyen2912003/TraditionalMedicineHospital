using AutoMapper;
using Project.Areas.Admin.Models.DTOs;
using Project.Areas.Admin.Models.Entities;
using Project.Areas.Admin.Models.ViewModels;

namespace Project.Mappers
{
    public class TreatmentMethodProfile : Profile
    {
        public TreatmentMethodProfile()
        {
            CreateMap<TreatmentMethod, TreatmentMethodDto>();
            CreateMap<TreatmentMethodDto, TreatmentMethod>()
                .ForMember(dest => dest.DepartmentId, opt => opt.MapFrom(src => src.DepartmentId));
            CreateMap<TreatmentMethod, TreatmentMethodViewModel>();
        }
    }
}
