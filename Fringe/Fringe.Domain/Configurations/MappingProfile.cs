using AutoMapper;
using Fringe.Domain.DTOs.VenueDTOs;
using Fringe.Domain.Entities;

//This class is to map objects directly

namespace Fringe.Domain.Configurations;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Venue mappings
        
        CreateMap<Venue, VenueDto>()
            .ForMember(dest => dest.LocationName, opt => opt.MapFrom(src => src.Location.LocationName))
            .ReverseMap();
        CreateMap<Venue, CreateVenueDto>().ReverseMap();
    }
}