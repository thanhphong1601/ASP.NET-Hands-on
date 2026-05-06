using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTO
{
    public class PagedResult<T>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public IEnumerable<T> Items { get; set; } = new List<T>();
    }
}
