﻿using AutoMapper;
using Identity.Application.Features.Authentication;
using Identity.Domain.Entities;

namespace Identity.Application.Mapping;

public class IdentityProfile : Profile
{
    public IdentityProfile()
    {
        CreateMap<RegisterDto, ApplicationUser>()
            .ForMember(
                dest => dest.UserName,
                src => src.MapFrom(x => x.Username))
            .ForMember(
                dest => dest.SecurityStamp,
                src => Guid.NewGuid().ToString())
            .ForMember(
                dest => dest.PhoneNumber,
                src => src.MapFrom(x => x.PhoneNumber ?? null))
            .ForMember(
                dest => dest.Address,
                src => src.Ignore())
            .ForMember(
                dest => dest.IsActive,
                src => src.MapFrom(x => true));
    }
}
