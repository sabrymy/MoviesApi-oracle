using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using MoviesApi.Entitities;
using MoviesApi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.HttpSys;
using AutoMapper;
using MoviesApi.Filters;
using Microsoft.EntityFrameworkCore;
using MoviesApi.DTOs;

namespace MoviesApi.Controllers
{
    //url end point start by convention with api
    //endUrl BaseUrl/api/Genres   where /api/Genres is the endpoint baseUrl https://localhost:44377/ OR webserver, Genres is the controller
    [Route("Genres")]
     [ApiController]
    public class GenresController : ControllerBase
    {
        
       
      
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
       public GenresController( ApplicationDbContext context, IMapper mapper)
        {
           
            this.context = context;
            this.mapper = mapper;
           
        }

        //[HttpGet("list] api/Genres/list
        [HttpGet]
        public async Task<List<GenreDTO>> Get()
        {
            
            var genres = await context.Genres.AsNoTracking().ToListAsync();
        var genreDto =   mapper.Map<List<GenreDTO>>(genres);
            return genreDto;
        }
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] GenreCreationDTO genreCreationDTO)

        {
            //ModelState used in validation if [ApiController] not specified for controller class
            // if (!ModelState.IsValid)
            //  {
            //    return BadRequest(ModelState);
            // }

            if (genreCreationDTO == null)
            {

                return NoContent();
            }
            else
            {
                var genre = mapper.Map<Genre>(genreCreationDTO);
                context.Genres.Add(genre);
                await context.SaveChangesAsync();
                var genreDTO = mapper.Map<GenreDTO>(genre);
                 return new CreatedAtRouteResult("getGenre", new { Id = genreDTO.Id }, genreDTO);

            }
           
        }

       
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id,   [FromBody] GenreCreationDTO genreCreationDTO)
        {
            var genre = mapper.Map<Genre>(genreCreationDTO);
            genre.Id = id;
            context.Entry<Genre>(genre).State = EntityState.Modified;
            await context.SaveChangesAsync();
            return NoContent();
        }
        [HttpDelete("{id}")]
        public async    Task<ActionResult> Delete(int id)
        {
            var exist = await context.Genres.AnyAsync(gen => gen.Id == id);
            if (exist)
            {
                context.Remove(new Genre() { Id = id });
                await context.SaveChangesAsync(); 
                return NoContent();
            }
            else
            {
                return NotFound();
            }
           
        }

        
        [HttpGet("{Id:int}", Name ="getGenre")]
        //https:localhost:port/api/Genres/int-value1/int-value2

        //  [HttpGet("list")]
        //  https://localhost:44377/api/Genres/list?Id=1&Status=2
        //  [Authorithe(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme )]
        //  finds custom filter whose type is typeof(MyActionFilter)
      
        public async Task<ActionResult<GenreDTO>> GetById([BindRequired] int Id)
        {
            //ModelState used in validation if [ApiController] not specified for controller class
            //   if (!ModelState.IsValid)
            //   {
            //      return BadRequest(ModelState);
            //  }


            var genre = await  context.Genres.AsNoTracking().FirstOrDefaultAsync(genre => genre.Id == Id);

               
            if ( genre == null )
            {
                
             //   throw new ApplicationException();
                return  NotFound();
            }
            var genreDTO = mapper.Map<Genre, GenreDTO>(genre);

            return Ok(genreDTO);
        }

    }

}

