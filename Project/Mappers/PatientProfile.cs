using AutoMapper;
using Project.Areas.Staff.Models.DTOs;
using Project.Areas.Staff.Models.Entities;

namespace Project.Mappers
{
    public class PatientProfile : Profile
    {
        public PatientProfile()
        {
            CreateMap<Patient, PatientDto>();
            CreateMap<PatientDto, Patient>();
        }
    }
}
