using AutoMapper;
using Project.Areas.Admin.Models.DTOs;
using Project.Areas.Admin.Models.Entities;
using Project.Areas.Admin.Models.ViewModels;

namespace Project.Mappers
{
    public class RoomProfile : Profile
    {
        public RoomProfile()
        {
            CreateMap<Room, RoomDto>();
            CreateMap<RoomDto, Room>()
                .ForMember(dest => dest.TreatmentMethod, opt => opt.Ignore())
                .ForMember(dest => dest.Department, opt => opt.Ignore());
            CreateMap<Room, RoomViewModel>()
                .ForMember(dest => dest.TreatmentMethodName,
                    opt => opt.MapFrom(src => src.TreatmentMethod != null ? src.TreatmentMethod.Name : "Không xác định"))
                .ForMember(dest => dest.DepartmentName,
                    opt => opt.MapFrom(src => src.Department != null ? src.Department.Name : "Không xác định"))
                .ForMember(dest => dest.TreatmentMethodId,
                    opt => opt.MapFrom(src => src.TreatmentMethod != null ? src.TreatmentMethod.Id : Guid.Empty))
                .ForMember(dest => dest.DepartmentId,
                    opt => opt.MapFrom(src => src.Department != null ? src.Department.Id : Guid.Empty));
        }
    }
}
