using AutoMapper;
using MoviesApi.DTOs;
using MoviesApi.Entitities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoviesApi.Helper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Genre, GenreDTO>().ReverseMap();
            CreateMap<GenreCreationDTO, Genre>();
            CreateMap<Person, PersonDTO>().ReverseMap();
            CreateMap<PersonCreationDTO, Person>().ReverseMap()
               .ForMember(x => x.picture, memberOptions => memberOptions.Ignore());
            CreateMap<Person, PersonPatchDTO>().ReverseMap();
            CreateMap<Movie, MovieDTO>().ReverseMap();
            CreateMap<MovieCreationDTO, Movie>().ForMember(x => x.Poster, memberOptions => memberOptions.Ignore())
            .ForMember(x => x.MoviesGenres, options => options.MapFrom(MapMoviesGenres))
            .ForMember(x => x.MoviesActors, options => options.MapFrom(MapMoviesActors));
            CreateMap<Movie, MoviePatchDTO>().ReverseMap();
        }
    
    
    private List<MoviesGenres> MapMoviesGenres(MovieCreationDTO movieCreationDTO, Movie movie)
        {
            var result = new List<MoviesGenres>();
            foreach(var id in movieCreationDTO.GenresIds)
            {
                result.Add(new MoviesGenres { GenreId = id });
            }
            return result;
        }

        private List<MoviesActors> MapMoviesActors(MovieCreationDTO movieCreationDTO, Movie movie)
        {
            var result = new List<MoviesActors>();
            foreach (var actorDTO in movieCreationDTO.Actors)
            {
                result.Add(new MoviesActors { PersonId = actorDTO.PersonId, Character= actorDTO.Character });
            }
            return result;
        }





    }
}