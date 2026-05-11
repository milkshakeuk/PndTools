using System.Text.Json;
using PndTools.Models;

namespace PndTools.Tests.Unit.Models;

public class PndToolsJsonContextTests
{
    private static readonly JsonSerializerOptions Options = new(PndToolsJsonContext.Default.Options);

    [Fact]
    public void Serialise_WillUseCamelCasePropertyNames()
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
        Assert.Contains("\"package\"", json);
        Assert.Contains("\"applications\"", json);
    }

    [Fact]
    public void Serialise_WillIncludeNestedProperties()
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
        Assert.Contains("\"id\"", json);
        Assert.Contains("test.app", json);
        Assert.Contains("My App", json);
    }

    [Fact]
    public void Serialise_WillSerialiseEmptyApplicationsAsEmptyArray()
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
        Assert.Contains("\"applications\":[]", json);
    }
}
