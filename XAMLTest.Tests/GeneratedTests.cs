using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Controls;

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

        [TestMethod]
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
                if (!targetType.GetConstructors().Any(c => c.GetParameters().Length == 0)) continue;

                IWindow window = await App.CreateWindowWithContent(@$"<{typeName} x:Name=""Thingy"" />");
                var typedGetElement = getElementMethod.MakeGenericMethod(targetType);

                dynamic task = typedGetElement.Invoke(window, new object[] { "Thingy" })!;
                var element = await task;

                foreach(var getMethod in extensionClass.GetMethods().Where(x => x.Name.StartsWith("Get")))
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
