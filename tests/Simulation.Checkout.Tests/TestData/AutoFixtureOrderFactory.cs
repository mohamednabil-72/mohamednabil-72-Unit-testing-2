using AutoFixture;
using Simulation.Checkout.Domain;

namespace Simulation.Checkout.Tests.TestData;

public static class AutoFixtureOrderFactory
{
    /// <summary>
    /// Creates an Order with AutoFixture, customized so every item has
    /// Quantity &gt; 0 and UnitPrice &gt;= 0 (valid for checkout).
    /// </summary>
    public static Order CreateValidOrder(int itemCount = 3)
    {
        var fixture = new Fixture();
        var random = new Random(42);

        fixture.Customize<OrderItem>(composer => composer
            .With(item => item.Quantity, () => random.Next(1, 10))
            .With(item => item.UnitPrice, () => (decimal)random.Next(0, 500)));

        fixture.Customize<Order>(composer => composer
            .With(order => order.Items,
                () => (IReadOnlyList<OrderItem>)fixture.CreateMany<OrderItem>(itemCount).ToList()));

        return fixture.Create<Order>();
    }
}
