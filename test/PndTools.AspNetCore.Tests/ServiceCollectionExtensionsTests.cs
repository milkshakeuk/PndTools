// Copyright (c) milkshakeuk. All rights reserved.
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.DependencyInjection;
using PndTools.Validation;

namespace PndTools.AspNetCore.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddPndTools_RegistersIPxmlParser_AsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddPndTools();

        // Assert
        var descriptor = Assert.Single(services, d => d.ServiceType == typeof(IPxmlParser));
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddPndTools_RegistersIPxmlValidator_AsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddPndTools();

        // Assert
        var descriptor = Assert.Single(services, d => d.ServiceType == typeof(IPxmlValidator));
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddPndTools_RegistersIPndArchiveFactory_AsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddPndTools();

        // Assert
        var descriptor = Assert.Single(services, d => d.ServiceType == typeof(IPndArchiveFactory));
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddPndTools_CalledTwice_DoesNotDuplicateRegistrations()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddPndTools();
        services.AddPndTools();

        // Assert
        Assert.Single(services, d => d.ServiceType == typeof(IPxmlParser));
        Assert.Single(services, d => d.ServiceType == typeof(IPxmlValidator));
        Assert.Single(services, d => d.ServiceType == typeof(IPndArchiveFactory));
    }

    [Fact]
    public void AddPndTools_NullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection services = null!;

        // Act
        // Assert
        Assert.Throws<ArgumentNullException>(() => services.AddPndTools());
    }

    [Fact]
    public void AddPndTools_WhenIPxmlParserAlreadyRegistered_DoesNotOverrideExistingRegistration()
    {
        // Arrange
        var services = new ServiceCollection();
        var customParser = new CustomParser();
        services.AddSingleton<IPxmlParser>(customParser);

        // Act
        services.AddPndTools();

        // Assert
        using var provider = services.BuildServiceProvider();
        var resolved = provider.GetRequiredService<IPxmlParser>();
        Assert.Same(customParser, resolved);
    }

    private sealed class CustomParser : IPxmlParser
    {
        public PndTools.Models.Pxml Parse(string xml) => throw new NotImplementedException();
    }
}
