namespace IFMS.Messaging.Events;

/// <summary>Published by Sales API when a transaction is recorded.</summary>
public record SaleRecorded(
    Guid TransactionId,
    Guid StationId,
    string FuelType,
    decimal Quantity,
    decimal TotalAmount,
    string CustomerName,
    DateTime TransactionDate
);
