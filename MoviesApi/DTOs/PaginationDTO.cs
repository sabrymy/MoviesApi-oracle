using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//The page will be displayed to user and number of records in page
namespace MoviesApi.DTOs
{
    public class PaginationDTO
{
        //The default page
        public int Page { get; set; } = 1;
        //number of records per page
        private int recordsPerPage = 10;
        private readonly int maxRecordsPerPage = 50;
        public int RecordsPerpage { get { return RecordsPerpage;  } set { recordsPerPage = (value > maxRecordsPerPage) ? maxRecordsPerPage : value; } }

    }
}
