using AutoMapper;
using Project.Areas.Admin.Models.DTOs;
using Project.Areas.Admin.Models.Entities;
using Project.Areas.Admin.Models.ViewModels;

namespace Project.Mappers
{
    public class MedicineProfile : Profile
    {
        public MedicineProfile()
        {
            CreateMap<Medicine, MedicineViewModel>()
                .ForMember(dest => dest.CategoryName,
                          opt => opt.MapFrom(src => src.MedicineCategory != null ? src.MedicineCategory.Name : "Không xác định"));

            CreateMap<Medicine, MedicineDto>()
                .ForMember(dest => dest.CategoryName,
                          opt => opt.MapFrom(src => src.MedicineCategory != null ? src.MedicineCategory.Name : "Không xác định"))
                .ForMember(dest => dest.MedicineCategoryId,
                          opt => opt.MapFrom(src => src.MedicineCategory != null ? src.MedicineCategory.Id : Guid.Empty))
                .ForMember(dest => dest.ImageFile, opt => opt.Ignore());

            CreateMap<MedicineDto, Medicine>()
                .ForMember(dest => dest.MedicineCategory, opt => opt.Ignore())
                .ForMember(dest => dest.Images, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore());
        }
    }
}
