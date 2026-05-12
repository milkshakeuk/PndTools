// Copyright (c) milkshakeuk. All rights reserved.
// SPDX-License-Identifier: MIT

using System.Text.Json;
using PndTools.Models;

namespace PndTools.Tests.Unit.Models;

public class PndToolsJsonContextTests
{
    [Fact]
    public void Serialise_Pxml_UsesCamelCasePropertyNames()
    {
        // Arrange
        var pxml = new Pxml
        {
            Package = new Package { Id = "test.app" },
            Applications = []
        };

        // Act
        var json = JsonSerializer.Serialize(pxml, PndToolsJsonContext.Default.Pxml);

        // Assert
        Assert.Contains("\"package\"", json, StringComparison.Ordinal);
        Assert.Contains("\"applications\"", json, StringComparison.Ordinal);
    }

    [Fact]
    public void Serialise_PxmlWithNestedProperties_IncludesAllProperties()
    {
        // Arrange
        var pxml = new Pxml
        {
            Package = new Package
            {
                Id = "test.app",
                Titles = [new Title("en_US", "My App")]
            },
            Applications = []
        };

        // Act
        var json = JsonSerializer.Serialize(pxml, PndToolsJsonContext.Default.Pxml);

        // Assert
        Assert.Contains("\"id\"", json, StringComparison.Ordinal);
        Assert.Contains("test.app", json, StringComparison.Ordinal);
        Assert.Contains("My App", json, StringComparison.Ordinal);
    }

    [Fact]
    public void Serialise_PxmlWithEmptyApplications_SerialisesAsEmptyArray()
    {
        // Arrange
        var pxml = new Pxml
        {
            Package = new Package(),
            Applications = []
        };

        // Act
        var json = JsonSerializer.Serialize(pxml, PndToolsJsonContext.Default.Pxml);

        // Assert
        Assert.Contains("\"applications\":[]", json, StringComparison.Ordinal);
    }
}
