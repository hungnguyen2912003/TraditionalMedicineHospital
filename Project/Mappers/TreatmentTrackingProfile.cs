using AutoMapper;
using Project.Areas.Staff.Models.DTOs.TrackingDTO;
using Project.Areas.Staff.Models.Entities;
using Project.Areas.Staff.Models.ViewModels;
using System.Linq;

namespace Project.Mappers
{
    public class TreatmentTrackingProfile : Profile
    {
        public TreatmentTrackingProfile()
        {
            CreateMap<TreatmentTracking, TreatmentTrackingViewModel>()
                .ForMember(dest => dest.PatientName,
                    opt => opt.MapFrom(src => GetPatientName(src))
                );
            CreateMap<TreatmentTrackingDto, TreatmentTrackingViewModel>();
        }

        private static string GetPatientName(TreatmentTracking src)
        {
            var detail = src.TreatmentRecordDetails.FirstOrDefault();
            if (detail != null && detail.TreatmentRecord != null && detail.TreatmentRecord.Patient != null)
                return detail.TreatmentRecord.Patient.Name;
            return "";
        }
    }
}