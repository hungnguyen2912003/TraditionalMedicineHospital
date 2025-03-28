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
                .ForMember(dest => dest.MedicineCategoryName,
                            opt => opt.MapFrom(src => src.MedicineCategory != null ? src.MedicineCategory.Name : "N/A"))
                .ForMember(dest => dest.ImageFile, opt => opt.Ignore());
            CreateMap<MedicineDto, Medicine>()
                .ForMember(dest => dest.Images, opt => opt.Ignore());
        }
    }
}
