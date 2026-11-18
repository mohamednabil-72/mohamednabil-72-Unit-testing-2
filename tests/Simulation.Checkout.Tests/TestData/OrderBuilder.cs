using Simulation.Checkout.Domain;

namespace Simulation.Checkout.Tests.TestData;

/// <summary>Small helper to build orders with fixed, readable data.</summary>
public static class OrderBuilder
{
    public static OrderItem BuildItem(string name, decimal unitPrice, int quantity, Guid? productId = null) =>
        new()
        {
            ProductId = productId ?? Guid.NewGuid(),
            ProductName = name,
            UnitPrice = unitPrice,
            Quantity = quantity
        };

    public static Order BuildOrder(params OrderItem[] items) =>
        new()
        {
            CustomerName = "Test Customer",
            CustomerEmail = "customer@example.com",
            Items = items
        };

    public static Order BuildOrder(string customerName, string customerEmail, params OrderItem[] items) =>
        new()
        {
            CustomerName = customerName,
            CustomerEmail = customerEmail,
            Items = items
        };

    /// <summary>One item with quantity 1, so the subtotal equals <paramref name="subtotal"/>.</summary>
    public static Order BuildOrderWithSubtotal(decimal subtotal) =>
        BuildOrder(BuildItem("Single Product", subtotal, 1));
}
