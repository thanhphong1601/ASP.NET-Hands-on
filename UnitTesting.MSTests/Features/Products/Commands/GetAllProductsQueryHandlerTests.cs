using ASP.NET_Hands_on.Application.CQRS.Products;
using ASP.NET_Hands_on.Application.DTO;
using ASP.NET_Hands_on.Application.IRepository;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTesting.MSTests.Features.Products.Commands;

[TestClass]
public sealed class GetAllProductsQueryHandlerTests
{
    [TestMethod]
    public async Task GetAllProductsQuery_WithZeroPagination_AppliesDefaultPaginationValuesAndReturnsAllProducts()
    {
        // Arrange
        var _mockRepo = new Mock<IProductRepository>();
        var _mockLogger = new Mock<ILogger<GetAllProductsQueryHandler>>();
        var _handler = new GetAllProductsQueryHandler(_mockRepo.Object, _mockLogger.Object);


        var query = new GetAllProductsQuery(-1, 5);
        var expectedItems = new List<ProductDto> { new ProductDto(1, "LAP1", "Laptop 1", 100),
        new ProductDto(2, "LAP2", "Laptop 2", 350),
        new ProductDto(3, "MSP1", "Mouse 1", 100),
        new ProductDto(4, "MON1", "Monitor 1", 500),
        new ProductDto(5, "KB1", "Keyboard 1", 75)
        };
        var expectedTotalCount = 5;

        _mockRepo
            .Setup(repo => repo.CountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTotalCount);
        _mockRepo
            .Setup(repo => repo.GetPagedProductDtosAsync(1, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedItems);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.AreEqual(expectedItems, result.Items);
        Assert.AreEqual(expectedTotalCount, result.TotalCount);

        _mockRepo.Verify(repo => repo.GetPagedProductDtosAsync(1, 5, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepo.Verify(repo => repo.CountAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

}

