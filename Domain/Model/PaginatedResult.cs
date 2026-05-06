using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Model
{
    public class PaginatedResult<T>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public IEnumerable<T> Items { get; set; }

        public PaginatedResult(IEnumerable<T> items, int count, int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalCount = count;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            Items = items;
        }

        public static PaginatedResult<T> Create(IEnumerable<T> items, int count, int pageNumber, int pageSize)
        {
            return new PaginatedResult<T>(items, count, pageNumber, pageSize);
        }
    }
}
