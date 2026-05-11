// Copyright (c) milkshakeuk. All rights reserved.
// SPDX-License-Identifier: MIT

using System.IO;
using System.Threading.Tasks;

namespace PndTools.Tests.Integration;

public class PxmlParserTests
{
    [Fact]
    public async Task Parse_WillParseValidPostHF6Pxml()
    {
        // Arrange
        var xml = await File.ReadAllTextAsync("Integration/TestCase/validPxml.xml", TestContext.Current.CancellationToken);

        // Act
        var pxml = PxmlParser.Parse(xml);

        // Assert
        Assert.Equal("packageId", pxml.Package.Id);
        Assert.Equal("1", pxml.Package.Version!.Major);
        Assert.Equal("0", pxml.Package.Version.Minor);
        Assert.Equal("0", pxml.Package.Version.Release);
        Assert.Equal("0", pxml.Package.Version.Build);
        Assert.Equal("release", pxml.Package.Version.Type);
        Assert.Equal("Package Author", pxml.Package.Author!.Name);
        Assert.Equal("http://www.example.org", pxml.Package.Author.Website);
        var title = Assert.Single(pxml.Package.Titles);
        Assert.Equal("en_US", title.Lang);
        Assert.Equal("Package Title", title.Text);
        var description = Assert.Single(pxml.Package.Descriptions);
        Assert.Equal("en_US", description.Lang);
        Assert.Equal("Package Description.", description.Text);
        Assert.Equal("icon.png", pxml.Package.Icon!.Path);
    }

    [Fact]
    public async Task Parse_WillParseValidPreHF6Pxml()
    {
        // Arrange
        var xml = await File.ReadAllTextAsync("Integration/TestCase/validPreHf6Pxml.xml", TestContext.Current.CancellationToken);

        // Act
        var pxml = PxmlParser.Parse(xml);

        // Assert
        var application = Assert.Single(pxml.Applications);
        var appTitle = Assert.Single(application.Titles);
        Assert.Equal("en_US", appTitle.Lang);
        Assert.Equal("Application Title", appTitle.Text);
        var appDescription = Assert.Single(application.Descriptions);
        Assert.Equal("en_US", appDescription.Lang);
        Assert.Equal("Application Description.", appDescription.Text);
    }
}
