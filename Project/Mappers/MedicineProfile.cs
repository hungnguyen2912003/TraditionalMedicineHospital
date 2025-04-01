using AutoMapper;
using Project.Areas.Admin.Models.DTOs;
using Project.Areas.Admin.Models.Entities;

namespace Project.Mappers
{
    public class MedicineProfile : Profile
    {
        public MedicineProfile()
        {
            CreateMap<Medicine, MedicineDto>()
                .ForMember(dest => dest.ImageFile, opt => opt.Ignore())
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.MedicineCategory.Name));

            CreateMap<MedicineDto, Medicine>()
                .ForMember(dest => dest.Images, opt => opt.Ignore())
                .ForMember(dest => dest.MedicineCategory, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore());
        }
    }
}
