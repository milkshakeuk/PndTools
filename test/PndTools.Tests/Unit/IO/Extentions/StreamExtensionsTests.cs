using System.IO;
using PndTools.IO.Extensions;
using PndTools.Tests.Helpers;

namespace PndTools.Tests.Unit.Extentions;

public class StreamExtensionsTests
{
    [Fact]
    public void Find_WillReturnMinusOneWhenStringNotFound()
    {
        // Arrange
        const int expected = -1;

        using Stream stream = StreamTestHelper.GenerateStreamFromString("fsdkl;dsfkl;dfkl;skl;");

        // Act
        var result = stream.Find("mystring");

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Find_WillReturnBytePositionWhenStringFound()
    {
        // Arrange
        const int expected = 10;

        using Stream stream = StreamTestHelper.GenerateStreamFromString("fsdkl;ds**mystring***skl;");

        // Act
        var result = stream.Find("mystring");

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Find_WillThrowArgumentNullExceptionWhenStreamIsNull()
    {
        // Arrange
        // Act
        // Assert
        Assert.Throws<ArgumentNullException>(() => (null as Stream)!.Find("mystring"));
    }

    [Fact]
    public void Find_WillThrowArgumentNullExceptionWhenInputIsNull()
    {
        // Arrange
        using Stream stream = StreamTestHelper.GenerateRandomStream();

        // Act
        // Assert
        Assert.Throws<ArgumentNullException>(() => stream.Find((string)null!));
    }

    [Fact]
    public void GetBytes_WillReturnByteArraySectionOfStreamFromStartAndEndPositionsSupplied()
    {
        // Arrange
        var expected = new byte[] { 120, 116 };

        using Stream stream = StreamTestHelper.GenerateStreamFromString("randomtext");

        // Act
        var result = stream.GetBytes(8, 10);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetBytes_WillThrowExceptionIfStartPositionLessThanZero()
    {
        // Arrange
        const string expectedParamName = "start";

        using Stream stream = StreamTestHelper.GenerateStreamFromString("randomtext");

        // Act
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => stream.GetBytes(-14, 10));

        // Assert
        Assert.Equal(expectedParamName, ex.ParamName);
    }

    [Fact]
    public void GetBytes_WillThrowExceptionIfEndPositionGreaterThanStreamLength()
    {
        // Arrange
        const string expectedParamName = "end";

        using Stream stream = StreamTestHelper.GenerateStreamFromString("randomtext");

        // Act
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => stream.GetBytes(8, 40));

        // Assert
        Assert.Equal(expectedParamName, ex.ParamName);
    }

    [Fact]
    public void GetBytes_WillThrowArgumentNullExceptionWhenTheStreamIsNull()
    {
        // Arrange
        // Act
        // Assert
        Assert.Throws<ArgumentNullException>(() => (null as Stream)!.GetBytes(8, 10));
    }

    [Fact]
    public void GetBytes_WillReturnByteArrayOfStream()
    {
        // Arrange
        var expected = StreamTestHelper.GenerateRandomBytes();

        using Stream stream = new MemoryStream(expected);

        // Act
        var result = stream.GetBytes();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Find_WillReturnLastOccurrenceWhenSearchingBackwards()
    {
        // Arrange
        const int expected = 13;

        using Stream stream = StreamTestHelper.GenerateStreamFromString("mystring_____mystring___");

        // Act
        var result = stream.Find("mystring", Direction.Backwards);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Find_WillReturnMinusOneWhenStringNotFoundSearchingBackwards()
    {
        // Arrange
        const int expected = -1;

        using Stream stream = StreamTestHelper.GenerateStreamFromString("fsdkl;dsfkl;dfkl;skl;");

        // Act
        var result = stream.Find("mystring", Direction.Backwards);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Find_WillReturnBytePositionWhenByteArrayFound()
    {
        // Arrange
        const int expected = 10;

        using Stream stream = StreamTestHelper.GenerateStreamFromString("fsdkl;ds**mystring***skl;");

        // Act
        var result = stream.Find(System.Text.Encoding.UTF8.GetBytes("mystring"));

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Find_WillReturnMinusOneWhenByteArrayNotFound()
    {
        // Arrange
        const int expected = -1;

        using Stream stream = StreamTestHelper.GenerateStreamFromString("fsdkl;dsfkl;dfkl;skl;");

        // Act
        var result = stream.Find(System.Text.Encoding.UTF8.GetBytes("mystring"));

        // Assert
        Assert.Equal(expected, result);
    }
}
