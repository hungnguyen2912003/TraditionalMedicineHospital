using AutoMapper;
using Project.Areas.Admin.Models.DTOs;
using Project.Areas.Admin.Models.Entities;

namespace Project.Mappers
{
    public class EmployeeCategoryProfile : Profile
    {
        public EmployeeCategoryProfile()
        {
            CreateMap<EmployeeCategory, EmployeeCategoryDto>();
            CreateMap<EmployeeCategoryDto, EmployeeCategory>();
        }
    }
}
