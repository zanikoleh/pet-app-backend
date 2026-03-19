using AutoMapper;
using PetService.Application.DTOs;
using PetService.Domain.Aggregates;
using PetService.Domain.Entities;

namespace PetService.Application.Mappings;

/// <summary>
/// AutoMapper profile for Pet Service DTOs.
/// </summary>
public class PetMappingProfile : Profile
{
    public PetMappingProfile()
    {
        // Pet mappings
        CreateMap<Pet, PetDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.Value))
            .ForMember(dest => dest.Breed, opt => opt.MapFrom(src => src.Breed != null ? src.Breed.Value : null))
            .ForMember(dest => dest.Photos, opt => opt.MapFrom(src => src.Photos))
            .ForMember(dest => dest.Documents, opt => opt.MapFrom(src => src.Documents));

        // Photo mappings
        CreateMap<Photo, PhotoDto>();

        // Document mappings
        CreateMap<Document, DocumentDto>();

        // Request to command mappings (if using CQRS with command objects)
        CreateMap<CreatePetRequest, Pet>();
    }
}
