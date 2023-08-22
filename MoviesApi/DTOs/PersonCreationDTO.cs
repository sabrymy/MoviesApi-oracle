using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoviesApi.Validation;

namespace MoviesApi.DTOs
{
    public class PersonCreationDTO : PersonPatchDTO
    {
       // [FileSizeValidation(5000)]
        [ContentTypeValidator(ContentTypeGroup.Image)]
        public IFormFile picture { get; set; }
    }
}
