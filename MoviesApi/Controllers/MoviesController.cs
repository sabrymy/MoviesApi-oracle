using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesApi.DTOs;
using MoviesApi.Entitities;
using MoviesApi.Helper;
using MoviesApi.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MoviesApi.Controllers
{
    [ApiController]
    [Route("movies")]
    public class MoviesController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IFileStorageService fileStorageService;
        private readonly string containerName = "movies";
       public MoviesController(ApplicationDbContext context, IMapper mapper,IFileStorageService fileStorageService)
        {
            this.context = context;
            this.fileStorageService = fileStorageService;
            this.mapper = mapper;
        }

        [HttpGet(Name ="getList")]
        public async Task<ActionResult<List<MovieDTO>>> Get()
        {
            var movies = await context.Movies.ToListAsync();
            return mapper.Map<List<MovieDTO>>(movies);

        }
        [HttpGet("filter")]
        public async Task<ActionResult<IndexMoviePageDTO>> GetWithFilter()
        {
            var top = 6;
            var today = DateTime.Today;
            var upcomingReleases = await context.Movies.
                Where(x => x.ReleaseDate > today)
                .OrderBy(x=>x.ReleaseDate)
                .Take(top)
                .ToListAsync();


            var inTheaters = await context.Movies.
               Where(x => x.InTheaters == true)
               .OrderBy(x => x.ReleaseDate)
               .Take(top)
               .ToListAsync();


            var result = new IndexMoviePageDTO();
            result.InTheaters = mapper.Map<List<MovieDTO>>(inTheaters);
            result.UpComingReleases = mapper.Map<List<MovieDTO>>(upcomingReleases);
           
            return result;

        }

        [HttpGet("filterByUser")]
        public async Task<ActionResult<List<MovieDTO>>> getByFilter([FromQuery] FilterMoviesDTO filterMoviesDTO )
        {
            var moviesQueryable = context.Movies.AsQueryable();
            if (!string.IsNullOrWhiteSpace(filterMoviesDTO.Title))
            {
                moviesQueryable = moviesQueryable.Where(x => x.Title.Contains(filterMoviesDTO.Title));
            }
            if (filterMoviesDTO.InTheaters)
            {
                moviesQueryable = moviesQueryable.Where(x => x.InTheaters);
            }
            if (filterMoviesDTO.UpcomingReleases)
            {
                var today = DateTime.Today;
                moviesQueryable = moviesQueryable.Where(x => x.ReleaseDate > today);
            }
            if (filterMoviesDTO.GenreId !=0)
            {
                moviesQueryable = moviesQueryable.Where(x => x.MoviesGenres.Select(y => y.GenreId).Contains(filterMoviesDTO.GenreId)) ;
            }
               await HttpContext.InsertPaginationParametersInResponse(moviesQueryable, filterMoviesDTO.RecordsPerPage);
             var movies = await moviesQueryable.Paginate(filterMoviesDTO.Pagination).ToListAsync();
           // var movies = await  moviesQueryable.ToListAsync();
           Console.WriteLine(movies.Count);
            return mapper.Map<List<MovieDTO>>(movies);

        }







        [HttpGet("{id}",Name ="getMovie")]
        public async Task<ActionResult<MovieDTO>> GetbyId(int id)
        {
            var movie = await context.Movies.FirstOrDefaultAsync(x => x.Id == id);
            if (movie == null) {
                return NotFound();
            }
            return mapper.Map<MovieDTO>(movie);

        }

        [HttpPost()]
        public async Task<ActionResult> Post([FromForm] MovieCreationDTO movieCreationDTO)
        {
            var movie = mapper.Map<Movie>(movieCreationDTO);
            
            if (movieCreationDTO.Poster != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await movieCreationDTO.Poster.CopyToAsync(memoryStream);
                    var content = memoryStream.ToArray();
                    var extension = movieCreationDTO.Poster.FileName.Split(".")[1];
                    string container = containerName;
                    movie.Poster = await fileStorageService.SaveFile(content, extension, container,
                        movieCreationDTO.Poster.ContentType);
                }
            }
            AnnotateActorsOrder(movie);
            context.Add(movie);
            await  context.SaveChangesAsync();
            var movieDTO = mapper.Map<MovieDTO>(movie);
            return new CreatedAtRouteResult("getMovie", new {id = movie.Id }, movieDTO);

        }

        private static void AnnotateActorsOrder(Movie movie)
        {
            if (movie.MoviesActors != null)
            {
                for (int i=0; i < movie.MoviesActors.Count; i++) {
                    movie.MoviesActors[i].Order = i;
                }

            }

        } 


        
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromForm] MovieCreationDTO movieCreationDTO)
        {
            var movieDB = await context.Movies.FirstOrDefaultAsync(x => x.Id == id);
            if (movieDB == null)
            {
                return NotFound();
            }

            movieDB = mapper.Map(movieCreationDTO, movieDB);
            
            if (movieCreationDTO.Poster != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await movieCreationDTO.Poster.CopyToAsync(memoryStream);
                    var content = memoryStream.ToArray();
                    var extension = movieCreationDTO.Poster.FileName.Split(".")[1];
                    string container = containerName;
                    movieDB.Poster = await fileStorageService.SaveFile(content, extension, container,
                        movieCreationDTO.Poster.ContentType);
                }
            }
           
            string stmt = $"delete from MoviesActors where MovieId ={movieDB.Id }; delete from MoviesGenres where MovieId ={movieDB.Id }; ";

            //delete from \"TESTUSER\".\"MoviesGenres\" where \"MovieId\" ={movieDB.Id } 
            await context.Database.ExecuteSqlInterpolatedAsync($"delete from \"TESTUSER\".\"MoviesActors\" where \"MovieId\" ={movieDB.Id };  ");
            await context.Database.ExecuteSqlInterpolatedAsync($"delete from \"TESTUSER\".\"MoviesGenres\" where \"MovieId\" ={movieDB.Id}");
            
            AnnotateActorsOrder(movieDB);


            await context.SaveChangesAsync();
            return NoContent();


        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var exists = await context.Movies.AnyAsync(x => x.Id == id);
            if (!exists)
            {
                return NotFound();
            }
            context.Remove(new Movie { Id = id });
            await  context.SaveChangesAsync();
            return NoContent();

        }

        [HttpPatch("{id}")]
        public async Task<ActionResult> Patch(int id, [FromBody] JsonPatchDocument<MoviePatchDTO> patchDocument)
        {
            if (patchDocument == null)
            {
                return BadRequest();
            }
            var entityFromDB = await context.Movies.FirstOrDefaultAsync(x => x.Id == id);
            if (entityFromDB == null)
            {
                return NotFound();
            }
            var entityDTO = mapper.Map<MoviePatchDTO>(entityFromDB);
            //APPLY OPERATIONS IN JSONDOCUMENT ON ENTITYDTO WHICH OF TYPE MoviePatchDTOPatchDTO AND SAVE THE ERRORS IN DICTIONARY ModeLState
            patchDocument.ApplyTo(entityDTO, ModelState);
            //validate model with isnstance entityDTO against validation rules
            var isValid = TryValidateModel(entityDTO);
            if (!isValid)
            {
                return BadRequest(ModelState);
            }
            //reflect any changes in entityDTO in entityFromDB where entityFromDB is reflected in context
            mapper.Map(entityDTO, entityFromDB);
            await context.SaveChangesAsync();
            return NoContent();

        }




    }
}
