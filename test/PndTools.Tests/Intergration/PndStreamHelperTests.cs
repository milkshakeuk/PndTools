using System;
using System.IO;
using System.Text.RegularExpressions;
using PndTools;
using PndTools.Tests.Helpers;
using Xunit;

namespace PndTools.Tests.Intergration
{
    public class PndStreamHelperTests
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
                result = PndStreamHelper.GetPxml(stream);
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
                result = PndStreamHelper.GetIcon(stream);
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
                Assert.Throws<InvalidPndException>(() => PndStreamHelper.GetPxml(stream));
            }
        }

        [Fact]
        public void GetPxml_WillThrowNullArgumentExceptionWhenNullStreamIsSupplied()
        {
            // Assign
            // Action
            // Assert
            Assert.Throws<ArgumentNullException>(() => PndStreamHelper.GetPxml(null));
        }

        [Fact]
        public void GetIcon_WillThrowInvalidPndExceptionIfIconNotFound()
        {
            // Assign
            using (Stream stream = StreamTestHelper.GenerateRandomStream())
            {
                // Action
                // Assert
                Assert.Throws<InvalidPndException>(() => PndStreamHelper.GetIcon(stream));
            }
        }

        [Fact]
        public void GetIcon_WillThrowNullArgumentExceptionWhenNullStreamIsSupplied()
        {
            // Assign
            // Action
            // Assert
            Assert.Throws<ArgumentNullException>(() => PndStreamHelper.GetIcon(null));
        }

        private static string RemoveXmlDeclaration(string pxml)
        {
            return Regex.Replace(pxml, "^.+(\r\n|\r|\n)", string.Empty);
        }
    }
}