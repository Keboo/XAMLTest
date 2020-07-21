using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Media;
using XamlTest;

namespace XamlTest.Tests
{
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
    }
}
