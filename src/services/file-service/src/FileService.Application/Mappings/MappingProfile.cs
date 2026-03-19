using AutoMapper;
using FileService.Application.DTOs;
using FileService.Domain.Entities;

namespace FileService.Application.Mappings;

/// <summary>
/// AutoMapper profile for File Service DTOs.
/// </summary>
public class FileMappingProfile : Profile
{
    public FileMappingProfile()
    {
        CreateMap<FileRecord, FileDto>()
            .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.DeletedAt));
    }
}
