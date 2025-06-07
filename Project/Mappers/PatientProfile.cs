using AutoMapper;
using Project.Areas.Admin.Models.Entities;
using Project.Areas.NhanVien.Models.DTOs;

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
