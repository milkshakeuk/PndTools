using System;
using System.IO;
using PndTools.Extentions;
using PndToolsTests.Helpers;
using Xunit;

namespace PndToolsTests.Unit.Extentions
{
    public class StreamExtensionsTests
    {
        [Fact]
        public void Find_WillReturnMinusOneWhenStringNotFound()
        {
            // Assign
            const int expected = -1;
            long result;
            using (Stream stream = StreamTestHelper.GenerateStreamFromString("fsdkl;dsfkl;dfkl;skl;"))
            {
                // Action
                result = stream.Find("mystring");
            }

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Find_WillReturnBytePositionWhenStringFound()
        {
            // Assign
            const int expected = 10;
            long result;
            using (Stream stream = StreamTestHelper.GenerateStreamFromString("fsdkl;ds**mystring***skl;"))
            {
                // Action
                result = stream.Find("mystring");
            }

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetBytes_WillReturnByteArraySectionOfStreamFromStartAndEndPositionsSupplied()
        {
            // Assign
            var expected = new byte[] { 120, 116 };
            byte[] result;
            using (Stream stream = StreamTestHelper.GenerateStreamFromString("randomtext"))
            {
                // Action
                result = stream.GetBytes(8, 10);
            }

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetBytes_WillThrowExceptionIfStartPositionLessThanZero()
        {
            // Assign
            var expected = $"Start position cannot be less than 0.{Environment.NewLine}Parameter name: start";
            using (Stream stream = StreamTestHelper.GenerateStreamFromString("randomtext"))
            {
                // Action
                // Assert
                var ex = Assert.Throws<ArgumentOutOfRangeException>(() => stream.GetBytes(-14, 10));
                Assert.Equal(expected, ex.Message);
            }
        }

        [Fact]
        public void GetBytes_WillThrowExceptionIfEndPositionGreaterThanStreamLength()
        {
            // Assign
            var expected = $"End position cannot be greater than stream length (10).{Environment.NewLine}Parameter name: end";
            using (Stream stream = StreamTestHelper.GenerateStreamFromString("randomtext"))
            {
                // Action
                // Assert
                var ex = Assert.Throws<ArgumentOutOfRangeException>(() => stream.GetBytes(8, 40));
                Assert.Equal(expected, ex.Message);
            }
        }

        [Fact]
        public void GetBytes_WillReturnByteArrayOfStream()
        {
            // Assign
            var expected = StreamTestHelper.GenerateRandomBytes();
            byte[] result;
            using (Stream stream = new MemoryStream(expected))
            {
                // Action
                result = stream.GetBytes();
            }

            // Assert
            Assert.Equal(expected, result);
        }
    }
}