using oliejournal.lib;

namespace oliejournal.tests;

public class OlieCommonTests
{
    [Test]
    public void SafeFloat_ReturnsNull_Null()
    {
        // Arrange
        const string? value = null;

        // Act
        var result = value.SafeFloat();

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void SafeFloat_ReturnsNull_NotParseable()
    {
        // Arrange
        const string? value = "Dillon";

        // Act
        var result = value.SafeFloat();

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void SafeFloat_ReturnsFloat_Parseable()
    {
        // Arrange
        const string? value = "-3.14";

        // Act
        var result = value.SafeFloat();

        // Assert
        Assert.That(result, Is.EqualTo(-3.14).Within(0.001));
    }
}
