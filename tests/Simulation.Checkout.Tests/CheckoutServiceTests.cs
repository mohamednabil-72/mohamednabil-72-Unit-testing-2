
using FluentAssertions;
using Moq;
using Simulation.Checkout.Application;
using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Domain;
using Simulation.Checkout.Domain.Exceptions;
using Simulation.Checkout.Tests.TestData;

namespace Simulation.Checkout.Tests;

public class CheckoutServiceTests
{
    private readonly Mock<IOrderRepository> _repository = new();
    private readonly Mock<IInventoryService> _inventory = new();
    private readonly Mock<INotificationService> _notification = new();

    public CheckoutServiceTests()
    {
        _inventory
            .Setup(i => i.IsAvailableAsync(
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    private CheckoutService CreateSut() =>
        new(_repository.Object, _inventory.Object, _notification.Object);

    [Fact]
    public async Task CheckoutAsync_WhenOrderIsEmpty_ShouldThrowEmptyOrderException()
    {
        var order = OrderBuilder.BuildOrder();
        var sut = CreateSut();

        Func<Task> act = () => sut.CheckoutAsync(order);

        await act.Should().ThrowAsync<EmptyOrderException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CheckoutAsync_WhenQuantityIsZeroOrNegative_ShouldThrowInvalidQuantityExceptionWithProductName(
        int quantity)
    {
        var order = OrderBuilder.BuildOrder(
            OrderBuilder.BuildItem("Keyboard", 100m, quantity));
        var sut = CreateSut();

        Func<Task> act = () => sut.CheckoutAsync(order);

        await act.Should().ThrowAsync<InvalidQuantityException>()
            .WithMessage("*Keyboard*");
    }

    [Fact]
    public async Task CheckoutAsync_WhenPriceIsNegative_ShouldThrowInvalidPriceException()
    {
        var order = OrderBuilder.BuildOrder(
            OrderBuilder.BuildItem("Mouse", -0.01m, 1));
        var sut = CreateSut();

        Func<Task> act = () => sut.CheckoutAsync(order);

        await act.Should().ThrowAsync<InvalidPriceException>();
    }

    [Fact]
    public async Task CheckoutAsync_WhenPriceIsZero_ShouldSucceed()
    {
        var order = OrderBuilder.BuildOrder(
            OrderBuilder.BuildItem("Free Sticker", 0m, 1));
        var sut = CreateSut();

        var receipt = await sut.CheckoutAsync(order);

        receipt.Should().NotBeNull();
        receipt.Total.Should().Be(0m);
    }

    public static TheoryData<decimal, decimal> DiscountCases => new()
    {
        { 999.99m, 0m },
        { 1000m, 100m },
        { 1500m, 100m }
    };

    [Theory]
    [MemberData(nameof(DiscountCases))]
    public async Task CheckoutAsync_WhenSubtotalIsAroundThreshold_ShouldApplyExpectedDiscount(
        decimal subtotal,
        decimal expectedDiscount)
    {
        var order = OrderBuilder.BuildOrderWithSubtotal(subtotal);
        var sut = CreateSut();

        var receipt = await sut.CheckoutAsync(order);

        receipt.Discount.Should().Be(expectedDiscount);
    }

    [Fact]
    public async Task CheckoutAsync_WhenExampleA_ShouldReturnSubtotal700Discount0Total700()
    {
        var order = OrderBuilder.BuildOrder(
            OrderBuilder.BuildItem("Product 1", 150m, 2),
            OrderBuilder.BuildItem("Product 2", 400m, 1));
        var sut = CreateSut();

        var receipt = await sut.CheckoutAsync(order);

        receipt.Subtotal.Should().Be(700m);
        receipt.Discount.Should().Be(0m);
        receipt.Total.Should().Be(700m);
    }

    [Fact]
    public async Task CheckoutAsync_WhenExampleC_ShouldReturnSubtotal1500Discount100Total1400()
    {
        var order = OrderBuilder.BuildOrder(
            OrderBuilder.BuildItem("Product 1", 500m, 3));
        var sut = CreateSut();

        var receipt = await sut.CheckoutAsync(order);

        receipt.Subtotal.Should().Be(1500m);
        receipt.Discount.Should().Be(100m);
        receipt.Total.Should().Be(1400m);
    }

    [Fact]
    public async Task CheckoutAsync_WhenAllItemsInStock_ShouldSaveOnceAndSendConfirmationOnce()
    {
        var order = OrderBuilder.BuildOrder(
            OrderBuilder.BuildItem("Product 1", 150m, 2),
            OrderBuilder.BuildItem("Product 2", 400m, 1));
        var sut = CreateSut();

        var receipt = await sut.CheckoutAsync(order);

        receipt.Should().NotBeNull();

        _repository.Verify(
            r => r.SaveAsync(order, It.IsAny<CancellationToken>()),
            Times.Once);

        _notification.Verify(
            n => n.SendOrderConfirmationAsync(
                receipt,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CheckoutAsync_WhenOneItemIsOutOfStock_ShouldThrowAndNeverSaveOrSend()
    {
        var inStockItem = OrderBuilder.BuildItem("In Stock Product", 100m, 1);
        var outOfStockItem = OrderBuilder.BuildItem("Out Of Stock Product", 200m, 1);

        var order = OrderBuilder.BuildOrder(inStockItem, outOfStockItem);

        _inventory
            .Setup(i => i.IsAvailableAsync(
                outOfStockItem.ProductId,
                outOfStockItem.Quantity,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var sut = CreateSut();

        Func<Task> act = () => sut.CheckoutAsync(order);

        await act.Should().ThrowAsync<InsufficientInventoryException>()
            .WithMessage("*Out Of Stock Product*");

        _repository.Verify(
            r => r.SaveAsync(
                It.IsAny<Order>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _notification.Verify(
            n => n.SendOrderConfirmationAsync(
                It.IsAny<CheckoutReceipt>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CheckoutAsync_WhenValidationFails_ShouldNeverCallInventorySaveOrSend()
    {
        var order = OrderBuilder.BuildOrder(
            OrderBuilder.BuildItem("Keyboard", 100m, 0));
        var sut = CreateSut();

        Func<Task> act = () => sut.CheckoutAsync(order);

        await act.Should().ThrowAsync<InvalidQuantityException>();

        _inventory.Verify(
            i => i.IsAvailableAsync(
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _repository.Verify(
            r => r.SaveAsync(
                It.IsAny<Order>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _notification.Verify(
            n => n.SendOrderConfirmationAsync(
                It.IsAny<CheckoutReceipt>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CheckoutAsync_WhenOrderIsCreatedByAutoFixtureWithValidCustomization_ShouldSucceed()
    {
        var order = AutoFixtureOrderFactory.CreateValidOrder();
        var sut = CreateSut();

        var receipt = await sut.CheckoutAsync(order);

        receipt.OrderId.Should().Be(order.Id);
        receipt.Items.Should().HaveCount(order.Items.Count);
        receipt.Total.Should().BeGreaterThanOrEqualTo(0m);
    }

    [Fact]
    public async Task CheckoutAsync_WhenCustomerIsCreatedByBogusWithFixedSeed_ShouldReturnReceiptWithSameNameAndEmail()
    {
        var customer = BogusCustomerFactory.CreateCustomer();

        var order = OrderBuilder.BuildOrder(
            customer.Name,
            customer.Email,
            OrderBuilder.BuildItem("Product 1", 100m, 1));

        var sut = CreateSut();

        var receipt = await sut.CheckoutAsync(order);

        receipt.CustomerName.Should().Be(customer.Name);
        receipt.CustomerEmail.Should().Be(customer.Email);
    }
}
