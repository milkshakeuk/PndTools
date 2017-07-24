using System;
using System.IO;
using System.Text.RegularExpressions;
using PndTools.Tests.Helpers;
using Xunit;
using PndTools.IO.Extensions;

namespace PndTools.Tests.Intergration.IO.Extensions
{
    public class PndStreamExtensionsTests
    {
        [Fact]
        public void GetPxml_WillReturnPxmlFromPndFileStream()
        {
            // Assign
            string result;
            var pxmlStart = $"<?xml version=\"1.0\" encoding=\"UTF-8\"?>{Environment.NewLine}<PXML";
            var pxmlEnd = "</PXML>";

            using (Stream stream = File.OpenRead("Intergration/TestCase/SORR.pnd"))
            {
                // Action
                result = stream.GetPxml();
            }

            // Assert
            Assert.StartsWith(pxmlStart, result);
            Assert.EndsWith(pxmlEnd, result);
        }

        [Fact]
        public void GetIcon_WillReturnIconBytesFromPndFile()
        {
            // Assign
            byte[] result;
            var expected = File.ReadAllBytes("Intergration/TestExpectation/SORR.png");

            using (Stream stream = File.OpenRead("Intergration/TestCase/SORR.pnd"))
            {
                // Action
                result = stream.GetIcon();
            }

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetPxml_WillThrowInvalidPndExceptionIfPxmlNotFound()
        {
            // Assign
            using (Stream stream = StreamTestHelper.GenerateRandomStream())
            {
                // Action
                // Assert
                Assert.Throws<InvalidPndException>(() => stream.GetPxml());
            }
        }

        [Fact]
        public void GetPxml_WillThrowNullArgumentExceptionWhenNullStreamIsSupplied()
        {
            // Assign
            // Action
            // Assert
            Assert.Throws<ArgumentNullException>(() => (null as Stream).GetPxml());
        }

        [Fact]
        public void GetIcon_WillThrowInvalidPndExceptionIfIconNotFound()
        {
            // Assign
            using (Stream stream = StreamTestHelper.GenerateRandomStream())
            {
                // Action
                // Assert
                Assert.Throws<InvalidPndException>(() => stream.GetIcon());
            }
        }

        [Fact]
        public void GetIcon_WillThrowNullArgumentExceptionWhenNullStreamIsSupplied()
        {
            // Assign
            // Action
            // Assert
            Assert.Throws<ArgumentNullException>(() => (null as Stream).GetIcon());
        }

        private static string RemoveXmlDeclaration(string pxml)
        {
            return Regex.Replace(pxml, "^.+(\r\n|\r|\n)", string.Empty);
        }
    }
}