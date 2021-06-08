using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Controls;
using XamlTest;

[assembly: GenerateHelpers(typeof(System.Windows.Window))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.AccessText))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Border))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Button))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Calendar))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Canvas))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.CheckBox))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.ComboBox))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.ComboBoxItem))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.ContentControl))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.ContentPresenter))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.ContextMenu))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Control))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.DataGrid))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.DataGridCell))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.DataGridCellsPanel))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.DataGridRow))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.DatePicker))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Decorator))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.DockPanel))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.DocumentViewer))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Expander))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.FlowDocumentReader))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.FlowDocumentScrollViewer))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Frame))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Grid))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.GridSplitter))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.GridViewColumnHeader))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.GridViewHeaderRowPresenter))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.GridViewRowPresenter))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.GroupBox))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.GroupItem))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.HeaderedContentControl))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.HeaderedItemsControl))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Image))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.InkCanvas))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.InkPresenter))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.ItemsControl))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.ItemsPresenter))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Label))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.ListBox))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.ListBoxItem))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.ListView))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.ListViewItem))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.MediaElement))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Menu))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.MenuItem))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Page))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Panel))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.PasswordBox))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.ScrollContentPresenter))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.ProgressBar))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.RadioButton))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.RichTextBox))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.ScrollViewer))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Separator))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.FlowDocumentPageViewer))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Slider))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.StackPanel))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.TabControl))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.TabItem))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.TextBlock))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.TextBox))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.ToolBar))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.ToolBarTray))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.ToolTip))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.TreeView))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.TreeViewItem))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.UserControl))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Viewbox))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.VirtualizingPanel))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.VirtualizingStackPanel))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.WebBrowser))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.WrapPanel))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Primitives.BulletDecorator))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Primitives.ButtonBase))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Primitives.CalendarButton))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Primitives.CalendarDayButton))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Primitives.CalendarItem))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Primitives.CustomPopupPlacement))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Primitives.CustomPopupPlacementCallback))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Primitives.DataGridCellsPresenter))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Primitives.DataGridColumnHeader))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Primitives.DataGridColumnHeadersPresenter))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Primitives.DataGridDetailsPresenter))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Primitives.DataGridRowHeader))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Primitives.DataGridRowsPresenter))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Primitives.DatePickerTextBox))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Primitives.DocumentPageView))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Primitives.DocumentViewerBase))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Primitives.DragCompletedEventHandler))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Primitives.DragDeltaEventHandler))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Primitives.DragStartedEventHandler))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Primitives.GridViewRowPresenterBase))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Primitives.ItemsChangedEventHandler))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Primitives.LayoutInformation))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Primitives.MenuBase))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Primitives.MultiSelector))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Primitives.Popup))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Primitives.RangeBase))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Primitives.RepeatButton))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Primitives.ResizeGrip))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Primitives.ScrollBar))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Primitives.ScrollEventHandler))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Primitives.SelectiveScrollingGrid))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Primitives.Selector))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Primitives.StatusBar))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Primitives.StatusBarItem))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Primitives.TabPanel))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Primitives.TextBoxBase))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Primitives.Thumb))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Primitives.TickBar))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Primitives.ToggleButton))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Primitives.ToolBarOverflowPanel))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Primitives.ToolBarPanel))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Primitives.Track))]
[assembly: GenerateHelpers(typeof(System.Windows.Controls.Primitives.UniformGrid))]

namespace XamlTest.Tests
{

    [TestClass]
    public class GeneratedTests
    {
        public TestContext TestContext { get; set; } = null!;

        [NotNull]
        private IApp? App { get; set; }

        [TestInitialize]
        public async Task TestInitialize()
        {
            App = XamlTest.App.StartRemote(logMessage: msg => TestContext.WriteLine(msg));

            await App.InitializeWithDefaults(Assembly.GetExecutingAssembly().Location);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            App.Dispose();
        }

        [TestMethod, Ignore]
        public async Task CanInvokeGeneratedHelperMethods()
        {
            var extensionMethods = typeof(IVisualElement).Assembly.GetExportedTypes()
                .Where(x => x.IsAbstract && x.IsSealed && x.Name.EndsWith("GeneratedExtensions"));
            var targetAssembly = typeof(Button).Assembly;

            MethodInfo getElementMethod = typeof(IVisualElement).GetMethods()
                .Single(x => x.IsGenericMethod);

            foreach(var extensionClass in extensionMethods)
            {
                string typeName = extensionClass.Name[0..^19];
                
                Type? targetType = targetAssembly.GetType($"System.Windows.Controls.{typeName}")
                    ?? targetAssembly.GetType($"System.Windows.Controls.Primatives.{typeName}");

                if (targetType is null) continue;
                if (targetType == typeof(AdornedElementPlaceholder)) continue;
                if (targetType == typeof(ContextMenu)) continue;
                if (!targetType.GetConstructors().Any(c => c.GetParameters().Length == 0)) continue;

                IWindow window = await App.CreateWindowWithContent(@$"<{typeName} x:Name=""Thingy"" />");
                var typedGetElement = getElementMethod.MakeGenericMethod(targetType);

                dynamic task = typedGetElement.Invoke(window, new object[] { "Thingy" })!;
                var element = await task;

                foreach(var getMethod in extensionClass.GetMethods()
                    .Where(x => x.Name.StartsWith("Get") && x.IsStatic))
                {
                    //NB: Just validating we can invoke all get methods
                    //This ensures that all types can be serialized
                    try
                    {
                        await (Task)getMethod.Invoke(null, new[] { element })!;
                    }
                    catch(Exception e)
                    {
                        throw new Exception($"Failed invoking {getMethod.Name} on {extensionClass.Name}", e);
                    }
                }
            }
        }
    }
}
