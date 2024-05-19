using AutoMapper;
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
                src => src.MapFrom(x => x.Username));
    }
}
