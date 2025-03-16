using AutoMapper;
using Project.Areas.Admin.Models.DTOs;
using Project.Areas.Admin.Models.Entities;

namespace Project.Mappers
{
    public class MedicineCategoryProfile : Profile
    {
        public MedicineCategoryProfile()
        {
            CreateMap<MedicineCategory, MedicineCategoryDto>();
            CreateMap<MedicineCategoryDto, MedicineCategory>()
                .ForMember(dest => dest.Images, opt => opt.Ignore());
        }
    }
}
