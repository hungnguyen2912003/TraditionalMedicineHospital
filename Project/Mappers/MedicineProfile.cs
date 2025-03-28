using AutoMapper;
using Project.Areas.Admin.Models.DTOs;
using Project.Areas.Admin.Models.Entities;

namespace Project.Mappers
{
    public class MedicineProfile : Profile
    {
        public MedicineProfile()
        {
            CreateMap<Medicine, MedicineDto>();
            CreateMap<MedicineDto, Medicine>()
                .ForMember(dest => dest.Images, opt => opt.Ignore());
        }
    }
}
