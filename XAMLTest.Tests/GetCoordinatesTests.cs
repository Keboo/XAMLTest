using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace XamlTest.Tests
{
    [TestClass]
    public class GetCoordinatesTests
    {
        [NotNull]
        private static IApp? App { get; set; }

        [NotNull]
        private static IWindow? Window { get; set; }

        [ClassInitialize]
        public static async Task ClassInitialize(TestContext context)
        {
            App = XamlTest.App.StartRemote(logMessage: msg => context.WriteLine(msg));

            await App.InitializeWithDefaults(Assembly.GetExecutingAssembly().Location);

            Window = await App.CreateWindowWithContent(@"<Border />");
        }

        [ClassCleanup]
        public static void TestCleanup()
        {
            App.Dispose();
        }

        [TestMethod]
        public async Task OnGetCoordinate_ReturnsScreenCoordinatesOfElement()
        {
            await Window.SetXamlContent(@"<Border x:Name=""MyBorder"" 
Width=""30"" Height=""40"" VerticalAlignment=""Top"" HorizontalAlignment=""Left""/>");
            
            IVisualElement<Border> element = await Window.GetElement<Border>("MyBorder");

            Rect initialCoordinates = await element.GetCoordinates();
            await element.SetWidth(90);
            await element.SetHeight(80);
            await element.SetMargin(new Thickness(30));

            Rect newCoordinates = await element.GetCoordinates();
            Assert.AreEqual(3.0, Math.Round(newCoordinates.Width / initialCoordinates.Width));
            Assert.AreEqual(2.0, Math.Round(newCoordinates.Height / initialCoordinates.Height));
            Assert.AreEqual(initialCoordinates.Width, Math.Round(newCoordinates.Left - initialCoordinates.Left));
            Assert.AreEqual(initialCoordinates.Width, Math.Round(newCoordinates.Top - initialCoordinates.Top));
        }

        [TestMethod]
        public async Task OnGetCoordinate_ReturnsFractionalCoordinatesOfElement()
        {
            await Window.SetXamlContent(@"<Border x:Name=""MyBorder"" 
Width=""30"" Height=""40"" VerticalAlignment=""Top"" HorizontalAlignment=""Left""/>");
            
            IVisualElement<Border> element = await Window.GetElement<Border>("MyBorder");

            Rect initialCoordinates = await element.GetCoordinates();
            await element.SetWidth(30.7);
            await element.SetHeight(40.3);
            await element.SetMargin(new Thickness(0.1));

            Rect newCoordinates = await element.GetCoordinates();
            Assert.AreEqual(30.7, Math.Round(newCoordinates.Width, 5));
            Assert.AreEqual(40.3, Math.Round(newCoordinates.Height, 5));
            Assert.AreEqual(0.1, Math.Round(newCoordinates.Left - initialCoordinates.Left, 5));
            Assert.AreEqual(0.1, Math.Round(newCoordinates.Top - initialCoordinates.Top, 5));
        }

        [TestMethod]
        public async Task OnGetCoordinate_ReturnsRotatedElementLocation()
        {
            await Window.SetXamlContent(@"
<Border x:Name=""MyBorder"" Width=""30"" Height=""40"" VerticalAlignment=""Top"" HorizontalAlignment=""Left"">
    <Border.LayoutTransform>
        <RotateTransform Angle=""90"" />
    </Border.LayoutTransform>
</Border>
");
            IVisualElement<Border> element = await Window.GetElement<Border>("MyBorder");

            Rect coordinates = await element.GetCoordinates();
            Assert.AreEqual(40, Math.Round(coordinates.Width, 5));
            Assert.AreEqual(30, Math.Round(coordinates.Height, 5));
        }
    }
}
