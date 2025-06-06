using AutoMapper;
using Project.Areas.Admin.Models.Entities;
using Project.Areas.BacSi.Models.DTOs;
using Project.Areas.BacSi.Models.ViewModels;

namespace Mappers
{
    public class PrescriptionProfile : Profile
    {
        public PrescriptionProfile()
        {
            CreateMap<Prescription, PrescriptionDto>();
            CreateMap<PrescriptionDto, Prescription>();
            CreateMap<Prescription, PrescriptionViewModel>()
                //.ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.TotalCost))
                .ForMember(dest => dest.TreatmentRecordCode, opt => opt.MapFrom(src => src.TreatmentRecord != null ? src.TreatmentRecord.Code : "Không xác định"))
                .ForMember(dest => dest.PatientName, opt => opt.MapFrom(src => src.TreatmentRecord != null ? src.TreatmentRecord.Patient.Name : "Không xác định"))
                .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.Employee != null ? src.Employee.Name : "Không xác định"));
            CreateMap<PrescriptionViewModel, Prescription>();
        }

    }
}
