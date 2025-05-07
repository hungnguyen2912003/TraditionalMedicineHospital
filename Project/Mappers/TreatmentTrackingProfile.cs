using AutoMapper;
using Project.Areas.Staff.Models.DTOs.TrackingDTO;
using Project.Areas.Staff.Models.Entities;
using Project.Areas.Staff.Models.ViewModels;

namespace Project.Mappers
{
    public class TreatmentTrackingProfile : Profile
    {
        public TreatmentTrackingProfile()
        {
            CreateMap<TreatmentTracking, TreatmentTrackingDto>();
            CreateMap<TreatmentTrackingDto, TreatmentTracking>();
            CreateMap<TreatmentTrackingDto, TreatmentTrackingViewModel>();
            CreateMap<TreatmentTrackingCreateDto, TreatmentTracking>();
        }
    }
}