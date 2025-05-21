using AutoMapper;
using Project.Areas.Admin.Models.DTOs;
using Project.Areas.Admin.Models.Entities;
using Project.Areas.Admin.Models.ViewModels;

namespace Project.Mappers
{
    public class EmployeeProfile : Profile
    {
        public EmployeeProfile()
        {
            CreateMap<Employee, EmployeeViewModel>()
                .ForMember(dest => dest.CategoryName,
                    opt => opt.MapFrom(src => src.EmployeeCategory != null ? src.EmployeeCategory.Name : "Không xác định"))
                .ForMember(dest => dest.DepartmentName,
                   opt => opt.MapFrom(src => src.Room.Department != null ? src.Room.Department.Name : "Không xác định"))
                .ForMember(dest => dest.TreatmentMethodName,
                   opt => opt.MapFrom(src => src.Room.TreatmentMethod != null ? src.Room.TreatmentMethod.Name : "Không xác định"))
                .ForMember(dest => dest.RoomName,
                   opt => opt.MapFrom(src => src.Room != null ? src.Room.Name : "Không xác định"));
            CreateMap<Employee, EmployeeDto>()
                .ForMember(dest => dest.EmployeeCategoryId,
                          opt => opt.MapFrom(src => src.EmployeeCategory != null ? src.EmployeeCategory.Id : Guid.Empty))
                .ForMember(dest => dest.DepartmentId,
                         opt => opt.MapFrom(src => src.Room.Department != null ? src.Room.Department.Id : Guid.Empty))
                .ForMember(dest => dest.ImageFile, opt => opt.Ignore())
                .ForMember(dest => dest.RoomId,
                         opt => opt.MapFrom(src => src.Room != null ? src.Room.Id : Guid.Empty))
                .ForMember(dest => dest.TreatmentMethodId,
                         opt => opt.MapFrom(src => src.Room.TreatmentMethod != null ? src.Room.TreatmentMethod.Id : Guid.Empty));
            CreateMap<EmployeeDto, Employee>()
                .ForMember(dest => dest.EmployeeCategory, opt => opt.Ignore())
                .ForMember(dest => dest.Images, opt => opt.Ignore())
                .ForMember(dest => dest.Room, opt => opt.Ignore());
        }
    }
}
