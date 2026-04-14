using System;
using System.Collections.Generic;

namespace ASP.NET_Hands_on.Model
{
    public class DiscountDay
    {
        public int Id { get; set; }
        public string DayName { get; set; } = string.Empty;

        // CreatedDate store date only (time component will be zeroed)
        public DateTime CreatedDate { get; set; }

        // from and to include time
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public List<DiscountDayProduct> DiscountDayProducts { get; set; } = new();
    }
}
