
using FluentAssertions;
using NetArchTest.Rules;
using Simulation.Checkout.Application;
using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Domain;
using Simulation.Checkout.Infrastructure;

namespace Simulation.Checkout.Tests;

public class ArchitectureTests
{
    private const string DomainNamespace = "Simulation.Checkout.Domain";
    private const string ApplicationNamespace = "Simulation.Checkout.Application";
    private const string InfrastructureNamespace = "Simulation.Checkout.Infrastructure";

    private static readonly System.Reflection.Assembly DomainAssembly =
        typeof(Order).Assembly;

    private static readonly System.Reflection.Assembly ApplicationAssembly =
        typeof(CheckoutService).Assembly;

    private static readonly System.Reflection.Assembly InfrastructureAssembly =
        typeof(InMemoryOrderRepository).Assembly;

    
    [Fact]
    public void Domain_WhenCheckingDependencies_ShouldNotDependOnApplicationOrInfrastructure()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(ApplicationNamespace, InfrastructureNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Domain must not depend on other layers. Failing types: {0}",
            string.Join(", ", result.FailingTypeNames ?? []));
    }

   
    [Fact]
    public void Application_WhenCheckingDependencies_ShouldNotDependOnInfrastructure()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Application must not depend on Infrastructure. Failing types: {0}",
            string.Join(", ", result.FailingTypeNames ?? []));
    }

   
    [Fact]
    public void Interfaces_WhenCheckingNames_ShouldStartWithI()
    {
        var result = Types.InAssemblies(
                [DomainAssembly, ApplicationAssembly, InfrastructureAssembly])
            .That()
            .AreInterfaces()
            .Should()
            .HaveNameStartingWith("I")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "All interfaces must start with 'I'. Failing types: {0}",
            string.Join(", ", result.FailingTypeNames ?? []));
    }

    
    [Fact]
    public void OrderRepositoryImplementations_WhenCheckingNames_ShouldEndWithRepository()
    {
        var result = Types.InAssemblies(
                [DomainAssembly, ApplicationAssembly, InfrastructureAssembly])
            .That()
            .ImplementInterface(typeof(IOrderRepository))
            .Should()
            .HaveNameEndingWith("Repository")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Classes implementing IOrderRepository must end with 'Repository'. Failing types: {0}",
            string.Join(", ", result.FailingTypeNames ?? []));
    }
}
