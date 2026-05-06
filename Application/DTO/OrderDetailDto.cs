using System.Collections.Generic;
using System;

namespace ASP.NET_Hands_on.Application.DTO
{
    public record OrderDetailDto
    (
        int OrderId,
        DateTime OrderDate,
        decimal TotalPrice,
        List<ProductDto> Products,
        string? CustomerName
    );
}
