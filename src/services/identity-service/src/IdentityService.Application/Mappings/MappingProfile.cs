using AutoMapper;
using IdentityService.Application.DTOs;
using IdentityService.Domain.Aggregates;
using IdentityService.Domain.Entities;

namespace IdentityService.Application.Mappings;

/// <summary>
/// AutoMapper profile for Identity Service DTOs.
/// </summary>
public class IdentityMappingProfile : Profile
{
    public IdentityMappingProfile()
    {
        // User mappings
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
            .ForMember(dest => dest.OAuthLinks, opt => opt.MapFrom(src =>
                src.OAuthLinks.Select(l => new OAuthLinkDto
                {
                    Provider = l.Provider,
                    LinkedAt = l.LinkedAt
                }).ToList()));

        // OAuth link mappings
        CreateMap<OAuthProviderLink, OAuthLinkDto>();
    }
}
