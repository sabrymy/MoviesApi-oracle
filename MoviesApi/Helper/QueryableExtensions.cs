using MoviesApi.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

//the goal of this class is to extend functionality for any object implement Iqueryable  to do pagniation function
namespace MoviesApi.Helper
{
    public static class QueryableExtensions
{
        public static IQueryable<T> Paginate<T>(this IQueryable<T> queryable, PaginationDTO pagination)
        {
            return ( queryable.Skip((pagination.Page - 1) *( pagination.RecordsPerpage)).Take(pagination.RecordsPerpage));
        }
}
}
