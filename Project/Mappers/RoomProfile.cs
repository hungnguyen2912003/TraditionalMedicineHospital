﻿using AutoMapper;
using Project.Areas.Admin.Models.DTOs;
using Project.Areas.Admin.Models.Entities;
using Project.Areas.Admin.Models.ViewModels;

namespace Project.Mappers
{
    public class RoomProfile : Profile
    {
        public RoomProfile()
        {
            CreateMap<Room, RoomDto>();
            CreateMap<RoomDto, Room>()
                .ForMember(dest => dest.TreatmentMethod, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore());
            CreateMap<Room, RoomViewModel>()
                .ForMember(dest => dest.TreatmentName,
                    opt => opt.MapFrom(src => src.TreatmentMethod != null ? src.TreatmentMethod.Name : "Không xác định"));
        }
    }
}
