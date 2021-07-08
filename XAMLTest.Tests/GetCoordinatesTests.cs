using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
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

            await App.InitializeWithResources(@"
<Color x:Key=""TestColor"">Red</Color>
<SolidColorBrush x:Key=""TestBrush"" Color=""#FF0000"" />",
                Assembly.GetExecutingAssembly().Location);

            Window = await App.CreateWindowWithContent(@"<Grid x:Name=""MyGrid"">
  <Grid.Resources>
    <Color x:Key=""GridColorResource"">Red</Color>
  </Grid.Resources>
</Grid>");

            Grid = await Window.GetElement<Grid>("MyGrid");
        }

        [ClassCleanup]
        public static void TestCleanup()
        {
            App.Dispose();
        }

    }
}
