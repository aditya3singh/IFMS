using IFMS.Inventory.Application.Commands;
using IFMS.Inventory.Application.DTOs;
using IFMS.Inventory.Application.Interfaces;
using IFMS.Inventory.Domain.Entities;
using Moq;

namespace IFMS.Inventory.Tests;

public class FuelStockCommandHandlerTests
{
    private readonly Mock<IFuelStockRepository> _repoMock = new();

    [Fact]
    public async Task CreateStock_ValidInput_ReturnsResponse()
    {
        _repoMock.Setup(r => r.AddAsync(It.IsAny<FuelStock>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        var handler = new FuelStockCommandHandler(_repoMock.Object);

        var result = await handler.CreateAsync(new CreateFuelStockRequest("Petrol", 1000, 105.50m, Guid.NewGuid()));

        Assert.Equal("Petrol", result.FuelType);
        Assert.Equal(1000, result.Quantity);
        Assert.Equal(105.50m, result.PricePerLitre);
        Assert.Equal("Available", result.Status);
    }

    [Fact]
    public async Task UpdateStock_ExistingId_ReturnsUpdatedResponse()
    {
        var stock = FuelStock.Create("Diesel", 500, 95.00m, Guid.NewGuid());
        _repoMock.Setup(r => r.GetByIdAsync(stock.Id)).ReturnsAsync(stock);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<FuelStock>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        var handler = new FuelStockCommandHandler(_repoMock.Object);

        var result = await handler.UpdateStockAsync(new UpdateStockRequest(stock.Id, 300));

        Assert.Equal(300, result.Quantity);
        Assert.Equal("Available", result.Status);
    }

    [Fact]
    public async Task UpdateStock_ToZero_StatusIsOutOfStock()
    {
        var stock = FuelStock.Create("CNG", 200, 80.00m, Guid.NewGuid());
        _repoMock.Setup(r => r.GetByIdAsync(stock.Id)).ReturnsAsync(stock);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<FuelStock>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        var handler = new FuelStockCommandHandler(_repoMock.Object);

        var result = await handler.UpdateStockAsync(new UpdateStockRequest(stock.Id, 0));

        Assert.Equal(0, result.Quantity);
        Assert.Equal("OutOfStock", result.Status);
    }

    [Fact]
    public async Task UpdateStock_NotFound_ThrowsException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((FuelStock?)null);
        var handler = new FuelStockCommandHandler(_repoMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.UpdateStockAsync(new UpdateStockRequest(Guid.NewGuid(), 100)));
    }

    [Fact]
    public async Task GetAll_ReturnsAllStocks()
    {
        var stocks = new List<FuelStock>
        {
            FuelStock.Create("Petrol", 1000, 105.50m, Guid.NewGuid()),
            FuelStock.Create("Diesel", 800, 95.00m, Guid.NewGuid())
        };
        _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(stocks);
        var handler = new FuelStockCommandHandler(_repoMock.Object);

        var result = await handler.GetAllAsync();

        Assert.Equal(2, result.Count);
    }
}

public class FuelStockEntityTests
{
    [Fact]
    public void Create_SetsAllFields()
    {
        var stationId = Guid.NewGuid();
        var stock = FuelStock.Create("Petrol", 1000, 105.50m, stationId);

        Assert.NotEqual(Guid.Empty, stock.Id);
        Assert.Equal("Petrol", stock.FuelType);
        Assert.Equal(1000, stock.Quantity);
        Assert.Equal(105.50m, stock.PricePerLitre);
        Assert.Equal("Available", stock.Status);
        Assert.Equal(stationId, stock.StationId);
    }

    [Fact]
    public void IsLowStock_Below500_ReturnsTrue()
    {
        var stock = FuelStock.Create("Petrol", 499, 105.50m, Guid.NewGuid());
        Assert.True(stock.IsLowStock());
    }

    [Fact]
    public void IsLowStock_Above500_ReturnsFalse()
    {
        var stock = FuelStock.Create("Petrol", 501, 105.50m, Guid.NewGuid());
        Assert.False(stock.IsLowStock());
    }

    [Fact]
    public void UpdateStock_ToZero_SetsOutOfStock()
    {
        var stock = FuelStock.Create("Diesel", 500, 95.00m, Guid.NewGuid());
        stock.UpdateStock(0);
        Assert.Equal("OutOfStock", stock.Status);
        Assert.Equal(0, stock.Quantity);
    }

    [Fact]
    public void UpdatePrice_ChangesPrice()
    {
        var stock = FuelStock.Create("Diesel", 500, 95.00m, Guid.NewGuid());
        stock.UpdatePrice(100.00m);
        Assert.Equal(100.00m, stock.PricePerLitre);
    }
}
