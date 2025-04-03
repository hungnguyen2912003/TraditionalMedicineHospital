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
                    opt => opt.MapFrom(src => src.EmployeeCategory != null ? src.EmployeeCategory.Name : "Không xác định"));
            CreateMap<Employee, EmployeeDto>()
                .ForMember(dest => dest.CategoryName,
                          opt => opt.MapFrom(src => src.EmployeeCategory != null ? src.EmployeeCategory.Name : "Không xác định"))
                .ForMember(dest => dest.EmployeeCategoryId,
                          opt => opt.MapFrom(src => src.EmployeeCategory != null ? src.EmployeeCategory.Id : Guid.Empty))
                .ForMember(dest => dest.ImageFile, opt => opt.Ignore());
            CreateMap<EmployeeDto, Employee>()
                .ForMember(dest => dest.EmployeeCategory, opt => opt.Ignore())
                .ForMember(dest => dest.Images, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore());
        }
    }
}
