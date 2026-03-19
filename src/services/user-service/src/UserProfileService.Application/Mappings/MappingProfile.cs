using AutoMapper;
using UserProfileService.Application.DTOs;
using UserProfileService.Domain.Aggregates;
using UserProfileService.Domain.Entities;

namespace UserProfileService.Application.Mappings;

/// <summary>
/// AutoMapper profile for User Profile Service DTOs.
/// </summary>
public class UserProfileMappingProfile : Profile
{
    public UserProfileMappingProfile()
    {
        CreateMap<UserProfile, UserProfileDto>()
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.Profile.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.Profile.LastName))
            .ForMember(dest => dest.Bio, opt => opt.MapFrom(src => src.Profile.Bio))
            .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.Profile.DateOfBirth))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Profile.PhoneNumber))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Profile.Address))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.Profile.City))
            .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Profile.Country))
            .ForMember(dest => dest.ProfilePictureUrl, opt => opt.MapFrom(src => src.Profile.ProfilePictureUrl));

        CreateMap<UserPreferences, NotificationPreferencesDto>();
    }
}
