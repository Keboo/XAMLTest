using System.Windows.Media;

namespace XamlTest.Tests;

[TestClass]
public class ColorMixinsTests
{
    [TestMethod]
    public void ContrastRatio()
    {
        float ratio = Colors.Black.ContrastRatio(Colors.White);

        //Actual value should be 21, allowing for floating point rounding errors
        Assert.IsTrue(ratio >= 20.9);
    }

    [TestMethod]
    public void FlattenOnto_ReturnsForegroundWhenItIsOpaque()
    {
        Color foreground = Colors.Red;
        Color background = Colors.Blue;

        Color flattened = foreground.FlattenOnto(background);

        Assert.AreEqual(Colors.Red, flattened);
    }

    [TestMethod]
    public void FlattenOnto_ReturnsMergedColorWhenForegroundIsTransparent()
    {
        Color foreground = Color.FromArgb(0x88, 0, 0, 0);
        Color background = Colors.White;

        Color flattened = foreground.FlattenOnto(background);

        Color expected = Color.FromRgb(0x76, 0x76, 0x76);
        Assert.AreEqual(expected, flattened);
    }
}
