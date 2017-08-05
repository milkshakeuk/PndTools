using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PndTools.Tests.Intergration
{
    public class PxmlParserTests
    {
        [Fact]
        public async Task Parse_WillParseValidPostHF6Pxml()
        {
            // Assign
            var xml = await File.ReadAllTextAsync("Intergration/TestCase/validPxml.xml");
            var sut = new PxmlParser();

            // Action 
            var pxml = sut.Parse(xml);

            // Assert
            Assert.Equal("packageId", pxml.Package.Id);
            Assert.Equal("1", pxml.Package.Version.Major);
            Assert.Equal("0", pxml.Package.Version.Minor);
            Assert.Equal("0", pxml.Package.Version.Release);
            Assert.Equal("0", pxml.Package.Version.Build);
            Assert.Equal("release", pxml.Package.Version.Type);
            Assert.Equal("Package Author", pxml.Package.Author.Name);
            Assert.Equal("http://www.example.org", pxml.Package.Author.Website);
            Assert.Equal(1, pxml.Package.Titles.Count());
            Assert.True(pxml.Package.Titles.All(title => title.Lang == "en_US" && title.Text == "Package Title"));
            Assert.Equal(1, pxml.Package.Descriptions.Count());
            Assert.True(pxml.Package.Descriptions.All(title => title.Lang == "en_US" && title.Text == "Package Description."));
            Assert.Equal("icon.png", pxml.Package.Icon.Path);
        }

        [Fact]
        public async Task Parse_WillParseValidPreHF6Pxml()
        {
            // Assign
            var xml = await File.ReadAllTextAsync("Intergration/TestCase/validPreHf6Pxml.xml");
            var sut = new PxmlParser();

            // Action 
            var pxml = sut.Parse(xml);

            // Assert
            Assert.Equal(1, pxml.Applications.Count());

            var appliation = pxml.Applications.First();

            Assert.Equal(1, appliation.Titles.Count());
            Assert.True(appliation.Titles.All(title => title.Lang == "en_US" && title.Text == "Application Title"));
            Assert.Equal(1, appliation.Descriptions.Count());
            Assert.True(appliation.Descriptions.All(title => title.Lang == "en_US" && title.Text == "Application Description."));
        }
    }
}
