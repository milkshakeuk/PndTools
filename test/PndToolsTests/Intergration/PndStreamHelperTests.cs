using System.IO;
using PndTools;
using PndToolsTests.Helpers;
using Xunit;

namespace PndToolsTests.Intergration
{
    public class PndStreamHelperTests
    {
        [Fact]
        public void FindPxml_WillReturnPxmlFromPndFileStream()
        {
            // Assign
            string result;
            var expected = File.ReadAllText("Intergration/TestExpectation/SORR.xml");
            using (Stream stream = File.OpenRead("Intergration/TestCase/SORR.pnd"))
            {
                // Action
                result = PndStreamHelper.GetPxml(stream);
            }

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void FindPxml_WillReturnIconBytesFromPndFile()
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
        public void FindPxml_WillThrowInvalidPndExceptionIfPxmlNotFound()
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
        public void FindPxml_WillThrowInvalidPndExceptionIfIconNotFound()
        {
            // Assign
            using (Stream stream = StreamTestHelper.GenerateRandomStream())
            {
                // Action
                // Assert
                Assert.Throws<InvalidPndException>(() => PndStreamHelper.GetIcon(stream));
            }
        }
    }
}