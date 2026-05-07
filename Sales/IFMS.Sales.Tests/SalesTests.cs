using IFMS.Sales.Application.Commands;
using IFMS.Sales.Application.DTOs;
using IFMS.Sales.Application.Interfaces;
using IFMS.Sales.Domain.Entities;
using Moq;

namespace IFMS.Sales.Tests;

public class TransactionCommandHandlerTests
{
    private readonly Mock<ITransactionRepository> _repoMock = new();

    [Fact]
    public async Task CreateTransaction_ValidInput_CalculatesTotalAmount()
    {
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Transaction>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        var handler = new TransactionCommandHandler(_repoMock.Object);

        var result = await handler.CreateAsync(new CreateTransactionRequest(
            Guid.NewGuid(), "Petrol", 10, 105.50m, "UPI", "Rahul"));

        Assert.Equal(1055.00m, result.TotalAmount);
        Assert.Equal("Completed", result.Status);
        Assert.Equal("Rahul", result.CustomerName);
    }

    [Fact]
    public async Task GetAll_ReturnsList()
    {
        var txns = new List<Transaction>
        {
            Transaction.Create(Guid.NewGuid(), "Petrol", 10, 105, "UPI", "A"),
            Transaction.Create(Guid.NewGuid(), "Diesel", 20, 95, "Cash", "B")
        };
        _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(txns);
        var handler = new TransactionCommandHandler(_repoMock.Object);

        var result = await handler.GetAllAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByStation_FiltersCorrectly()
    {
        var stationId = Guid.NewGuid();
        var txns = new List<Transaction>
        {
            Transaction.Create(stationId, "Petrol", 10, 105, "UPI", "A")
        };
        _repoMock.Setup(r => r.GetByStationIdAsync(stationId)).ReturnsAsync(txns);
        var handler = new TransactionCommandHandler(_repoMock.Object);

        var result = await handler.GetByStationAsync(stationId);

        Assert.Single(result);
        Assert.Equal(stationId, result[0].StationId);
    }

    [Fact]
    public async Task GetTotalRevenue_SumsCorrectly()
    {
        var stationId = Guid.NewGuid();
        var txns = new List<Transaction>
        {
            Transaction.Create(stationId, "Petrol", 10, 100, "UPI", "A"),
            Transaction.Create(stationId, "Diesel", 20, 90, "Cash", "B")
        };
        _repoMock.Setup(r => r.GetByStationIdAsync(stationId)).ReturnsAsync(txns);
        var handler = new TransactionCommandHandler(_repoMock.Object);

        var result = await handler.GetTotalRevenueAsync(stationId);

        Assert.Equal(2800m, result); // 10*100 + 20*90
    }
}

public class TransactionEntityTests
{
    [Fact]
    public void Create_CalculatesTotalAmount()
    {
        var txn = Transaction.Create(Guid.NewGuid(), "Petrol", 15, 100, "UPI", "Test");

        Assert.Equal(1500, txn.TotalAmount);
        Assert.Equal("Completed", txn.Status);
    }

    [Fact]
    public void Create_SetsAllFields()
    {
        var stationId = Guid.NewGuid();
        var txn = Transaction.Create(stationId, "Diesel", 20, 95, "Cash", "Customer1");

        Assert.NotEqual(Guid.Empty, txn.Id);
        Assert.Equal(stationId, txn.StationId);
        Assert.Equal("Diesel", txn.FuelType);
        Assert.Equal(20, txn.Quantity);
        Assert.Equal(95, txn.PricePerLitre);
        Assert.Equal(1900, txn.TotalAmount);
        Assert.Equal("Cash", txn.PaymentMethod);
        Assert.Equal("Customer1", txn.CustomerName);
    }
}
