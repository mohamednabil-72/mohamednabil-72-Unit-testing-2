
using Moq;
using Simulation.Checkout.Application;
using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Tests.TestData;
using VerifyXunit;
namespace Simulation.Checkout.Tests;


public class ReceiptSnapshotTests
{
    [Fact]
    public async Task CheckoutAsync_WhenExampleC_ShouldMatchApprovedReceiptSnapshot()
    {
        var inventory = new Mock<IInventoryService>();

        inventory
            .Setup(i => i.IsAvailableAsync(
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var sut = new CheckoutService(
            new Mock<IOrderRepository>().Object,
            inventory.Object,
            new Mock<INotificationService>().Object);

        var order = OrderBuilder.BuildOrder(
            "Ahmed Hassan",
            "ahmed.hassan@example.com",
            OrderBuilder.BuildItem("Mechanical Keyboard", 500m, 3));

        var receipt = await sut.CheckoutAsync(order);

        await Verify(receipt);
    }
}
