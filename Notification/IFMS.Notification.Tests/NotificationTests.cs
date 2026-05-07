using IFMS.Notification.API.Controllers;
using IFMS.Notification.API.DTOs;
using IFMS.Notification.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace IFMS.Notification.Tests;

public class NotifyControllerTests
{
    private readonly Mock<INotificationService> _notifyMock = new();
    private readonly Mock<ILogger<NotifyController>> _loggerMock = new();

    private NotifyController CreateController() =>
        new(_notifyMock.Object, _loggerMock.Object);

    [Fact]
    public async Task SendTokenNotification_CallsSmsAndEmail()
    {
        _notifyMock.Setup(n => n.SendSmsAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
        _notifyMock.Setup(n => n.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

        var controller = CreateController();
        var result = await controller.SendTokenNotification(new SendTokenNotificationRequest(
            "+919876543210", "cust@ifms.com", "Rahul", "IFM-01-12345678", "Shell Bandra", "Petrol", 10, 1050));

        var okResult = Assert.IsType<OkObjectResult>(result);
        _notifyMock.Verify(n => n.SendSmsAsync("+919876543210", It.IsAny<string>()), Times.Once);
        _notifyMock.Verify(n => n.SendEmailAsync("cust@ifms.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task SendLowStockAlert_CallsSmsAndEmail()
    {
        _notifyMock.Setup(n => n.SendSmsAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
        _notifyMock.Setup(n => n.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

        var controller = CreateController();
        var result = await controller.SendLowStockAlert(new SendLowStockAlertRequest(
            "dealer@ifms.com", "+919876543210", "HP Andheri", "Diesel", 150));

        var okResult = Assert.IsType<OkObjectResult>(result);
        _notifyMock.Verify(n => n.SendSmsAsync(It.IsAny<string>(), It.Is<string>(s => s.Contains("low"))), Times.Once);
    }

    [Fact]
    public async Task SendFraudAlert_CallsSmsAndEmail()
    {
        _notifyMock.Setup(n => n.SendSmsAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
        _notifyMock.Setup(n => n.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

        var controller = CreateController();
        var result = await controller.SendFraudAlert(new SendFraudAlertRequest(
            "admin@ifms.com", "+919876543210", "BP Juhu", "Unusually large transaction", 99999, DateTime.UtcNow));

        var okResult = Assert.IsType<OkObjectResult>(result);
        _notifyMock.Verify(n => n.SendSmsAsync(It.IsAny<string>(), It.Is<string>(s => s.Contains("FRAUD"))), Times.Once);
        _notifyMock.Verify(n => n.SendEmailAsync("admin@ifms.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }
}

public class MockNotificationServiceTests
{
    [Fact]
    public async Task SendSms_CompletesSuccessfully()
    {
        var logger = Mock.Of<ILogger<MockNotificationService>>();
        var service = new MockNotificationService(logger);

        // Should not throw
        await service.SendSmsAsync("+919876543210", "Test message");
    }

    [Fact]
    public async Task SendEmail_CompletesSuccessfully()
    {
        var logger = Mock.Of<ILogger<MockNotificationService>>();
        var service = new MockNotificationService(logger);

        // Should not throw
        await service.SendEmailAsync("test@ifms.com", "Test Subject", "<p>Test body</p>");
    }
}
