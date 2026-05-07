namespace IFMS.Sales.Application.DTOs;

public record CreateTransactionRequest(
    Guid StationId,
    string FuelType,
    decimal Quantity,
    decimal PricePerLitre,
    string PaymentMethod,
    string CustomerName
);

public record TransactionResponse(
    Guid Id,
    Guid StationId,
    string FuelType,
    decimal Quantity,
    decimal PricePerLitre,
    decimal TotalAmount,
    string PaymentMethod,
    string Status,
    DateTime TransactionDate,
    string CustomerName
);