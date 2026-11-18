using Bogus;

namespace Simulation.Checkout.Tests.TestData;

public sealed record CustomerData(string Name, string Email);

public static class BogusCustomerFactory
{
    public const int Seed = 12345;

    /// <summary>Realistic customer, deterministic because the seed is fixed.</summary>
    public static CustomerData CreateCustomer()
    {
        var faker = new Faker("en") { Random = new Randomizer(Seed) };

        return new CustomerData(faker.Name.FullName(), faker.Internet.Email());
    }
}
