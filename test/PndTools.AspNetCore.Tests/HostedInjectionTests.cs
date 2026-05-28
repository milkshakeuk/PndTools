// Copyright (c) milkshakeuk. All rights reserved.
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PndTools.Validation;

namespace PndTools.AspNetCore.Tests;

public class HostedInjectionTests
{
    private static IHost BuildHost()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureServices(services => services.AddPndTools())
            .Build();
    }

    [Fact]
    public async Task IPxmlParser_ResolvesAndParsesKnownFixture()
    {
        // Arrange
        using var host = BuildHost();
        await host.StartAsync(TestContext.Current.CancellationToken);
        var xml = await File.ReadAllTextAsync("TestCase/validPxml.xml", TestContext.Current.CancellationToken);
        var parser = host.Services.GetRequiredService<IPxmlParser>();

        // Act
        var pxml = parser.Parse(xml);

        // Assert
        Assert.Equal("packageId", pxml.Package.Id);
    }

    [Fact]
    public async Task IPxmlValidator_ResolvesAndValidatesKnownFixture()
    {
        // Arrange
        using var host = BuildHost();
        await host.StartAsync(TestContext.Current.CancellationToken);
        var xml = await File.ReadAllTextAsync("TestCase/validPxml.xml", TestContext.Current.CancellationToken);
        var validator = host.Services.GetRequiredService<IPxmlValidator>();

        // Act
        var result = validator.Validate(xml);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task IPndArchiveFactory_ResolvesAsSingleton()
    {
        // Arrange
        using var host = BuildHost();
        await host.StartAsync(TestContext.Current.CancellationToken);

        // Act
        var factory1 = host.Services.GetRequiredService<IPndArchiveFactory>();
        var factory2 = host.Services.GetRequiredService<IPndArchiveFactory>();

        // Assert
        Assert.Same(factory1, factory2);
    }
}
