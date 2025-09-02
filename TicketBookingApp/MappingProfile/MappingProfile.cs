using AutoMapper;
using TicketBookingApp.Dtos.HallDtos;
using TicketBookingApp.Dtos.MovieDtos;
using TicketBookingApp.Models;

namespace TicketBookingApp.MappingProfile
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CreateMovieDto, Movie>();
            CreateMap<UpdateMovieDto, Movie>();
            CreateMap<CreateHallDto, Hall>();
            CreateMap<UpdateHallDto, Hall>();
        }
    }
}
