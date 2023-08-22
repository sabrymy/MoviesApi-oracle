using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoviesApi.Helper
{
    //goal of this class to add total number of pages in response of header <T> generic to use with any entity
    //extension for Httpcontext class to use new method insertPaginationParametersInResponse
    public static class HttpContextExtensions
{
        public async static Task InsertPaginationParametersInResponse<T>(this HttpContext httpContext, IQueryable<T> queryable,  int recordsPerpage)
        {
            if (httpContext == null)
            {
                throw (new ArgumentNullException(nameof(httpContext)));
            }

            double count = await queryable.CountAsync();
            double totalAmountPages = Math.Ceiling(count / recordsPerpage);
            httpContext.Response.Headers.Add("totalAmountPages", totalAmountPages.ToString());
        }

}
}
