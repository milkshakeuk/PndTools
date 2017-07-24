using PndTools.Validation;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PndTools.Tests.Intergration.Validation
{
    public class PxmlValidatorTests
    {
        [Theory]
        [InlineData(null, typeof(ArgumentNullException))]
        [InlineData("", typeof(Guard.EmptyStringException))]
        [InlineData("  ", typeof(Guard.WhitespaceException))]
        public void Validate_WillThrowExceptionWhenPxmlIsNullOrEmpty(string pxml, Type exceptionType)
        {
            // Assign
            var sut = new PxmlValidator();

            // Action 
            // Assert
            Assert.Throws(exceptionType,() => sut.Validate(pxml));
        }

        [Fact]
        public async Task Validate_WillValidateValidPxmlIsValid()
        {
            // Assign
            var pxml = await File.ReadAllTextAsync("Intergration/TestCase/validPxml.xml");
            var sut = new PxmlValidator();

            // Action 
            var result = sut.Validate(pxml);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public async Task Validate_WillValidateInvValidPxmlIsNotValid()
        {
            // Assign
            var pxml = await File.ReadAllTextAsync("Intergration/TestCase/invalidPxml.xml");
            var sut = new PxmlValidator();
            var expectedErrors = new string[2] {
                "PXML The 'type' attribute is invalid - The value 'Release' is invalid according to its datatype 'releaseType' - The Enumeration constraint failed. (4:61)",
                "PXML The element 'package' has incomplete content. List of possible elements expected: 'titles'. (3:6)"
            };

            // Action 
            var result = sut.Validate(pxml);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal(2, result.Errors.Count());
            Assert.Contains(expectedErrors[0], result.Errors);
            Assert.Contains(expectedErrors[1], result.Errors);
        }
    }
}
