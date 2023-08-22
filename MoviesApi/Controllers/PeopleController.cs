using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoviesApi.Services;
using Microsoft.EntityFrameworkCore;
using MoviesApi.DTOs;
using System.IO;
using MoviesApi.Entitities;
using Microsoft.AspNetCore.JsonPatch;
using MoviesApi.Helper;

namespace MoviesApi.Controllers
{
    [ApiController]
    [Route("People")]
    public class PeopleController :ControllerBase
{
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IFileStorageService fileStorageService;
        private readonly string containerName = "people";

        public PeopleController(ApplicationDbContext context, IMapper mapper, IFileStorageService fileStorageService)
        {
            this.context = context;
            this.mapper = mapper;
            this.fileStorageService = fileStorageService;
        }

        [HttpGet]
        public async Task<ActionResult<List<PersonDTO>>> Get()
        {
            var people = await context.People.ToListAsync();
            return mapper.Map<List<PersonDTO>>(people);
        }

        [HttpGet("pagination")]
        public async Task<ActionResult<List<PersonDTO>>> GetPagination([FromQuery] PaginationDTO pagination)
        {
            var queryable = context.People.AsQueryable();
            await HttpContext.InsertPaginationParametersInResponse(queryable, pagination.RecordsPerpage);
          
            var people = await queryable.Paginate(pagination).ToListAsync();
            return mapper.Map<List<PersonDTO>>(people);
        }


        [HttpGet("{Id}", Name = "getPerson")]
        public async Task<ActionResult<PersonDTO>> Get(int id)
        {
            var person = await context.People.FirstOrDefaultAsync(x => x.Id == id);
            if (person == null)
                return NotFound();
            return mapper.Map<PersonDTO>(person);
        }
        [HttpPost]
        public async Task<ActionResult> Post([FromForm] PersonCreationDTO personCreationDTO)
        {
            var person = mapper.Map<Person>(personCreationDTO);
            if (personCreationDTO.picture != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await personCreationDTO.picture.CopyToAsync(memoryStream);
                    var content = memoryStream.ToArray();
                    var extension = personCreationDTO.picture.FileName.Split(".")[1];
                    string container = containerName;
                    person.Picture = await fileStorageService.SaveFile(content, extension, container,
                        personCreationDTO.picture.ContentType);
                }
            }


            context.Add(person);
            await context.SaveChangesAsync();
            var personDTO = mapper.Map<PersonDTO>(person);
            return new CreatedAtRouteResult("getPerson", new { Id = person.Id }, personDTO);

        }


        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromForm] PersonCreationDTO personCreationDTO)
        {
            var personDB = await context.People.FirstOrDefaultAsync(x => x.Id == id);
            personDB = mapper.Map(personCreationDTO, personDB);
            if (personCreationDTO.picture != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await personCreationDTO.picture.CopyToAsync(memoryStream);
                    var content = memoryStream.ToArray();
                    var extension = personCreationDTO.picture.FileName.Split(".")[1];
                    string container = containerName;
                    personDB.Picture = await fileStorageService.EditFile(content, extension, container, personDB.Picture,
                        personCreationDTO.picture.ContentType);
                }
            }

            await context.SaveChangesAsync();

            return NoContent();



        }
        [HttpPatch("{id}")]
        public async Task<ActionResult> Patch(int id, [FromBody] JsonPatchDocument<PersonPatchDTO> patchDocument)
        {
            if (patchDocument == null) {
                return BadRequest();
            }
            var entityFromDB = await context.People.FirstOrDefaultAsync(x => x.Id == id);
            if (entityFromDB == null)
            {
                return NotFound();
            }
            var entityDTO = mapper.Map<PersonPatchDTO>(entityFromDB);
            //APPLY OPERATIONS IN JSONDOCUMENT ON ENTITYDTO WHICH OF TYPE PersonPatchDTO AND SAVE THE ERRORS IN DICTIONARY ModeLState
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
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var exist = await context.People.AnyAsync(person => person.Id == id);
            if (exist)
            {
                context.Remove(new Person() { Id = id });
                await context.SaveChangesAsync();
                return NoContent();
            }
            else
            {
                return NotFound();
            }

        }



    }
}
