using AutoMapper;
using Project.Areas.Admin.Models.Entities;
using Project.Areas.YTa.Models.DTOs;
using Project.Areas.YTa.Models.ViewModels;

namespace Project.Mappers
{
    public class TreatmentTrackingProfile : Profile
    {
        public TreatmentTrackingProfile()
        {
            CreateMap<TreatmentTracking, TreatmentTrackingDto>();
            CreateMap<TreatmentTrackingDto, TreatmentTracking>();
            CreateMap<TreatmentTrackingDto, TreatmentTrackingViewModel>();
            CreateMap<TreatmentTrackingViewModel, TreatmentTrackingDto>();
            CreateMap<TreatmentTrackingCreateDto, TreatmentTracking>();
        }
    }
}