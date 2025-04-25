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
                //.ForMember(dest => dest.Department, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore());
            CreateMap<TreatmentMethod, TreatmentMethodViewModel>();
                //.ForMember(dest => dest.DepName,
                //    opt => opt.MapFrom(src => src.Department != null ? src.Department.Name : "Không xác định"));
        }
    }
}
