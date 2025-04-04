using AutoMapper;
using Project.Areas.Admin.Models.DTOs;
using Project.Areas.Admin.Models.Entities;

namespace Project.Mappers
{
    public class RegulationProfile : Profile
    {
        public RegulationProfile()
        {
            CreateMap<Regulation, RegulationDto>();
            CreateMap<RegulationDto, Regulation>();
        }
    }
}
