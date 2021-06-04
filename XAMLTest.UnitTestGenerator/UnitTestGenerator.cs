using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace XAMLTest.UnitTestGenerator
{
    [Generator]
    public class UnitTestGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
#if DEBUG
            if (!Debugger.IsAttached)
            {
                //Debugger.Launch();
            }
#endif
            SyntaxReceiver rx = (SyntaxReceiver)context.SyntaxContextReceiver!;

            foreach (TypeInfo targetType in rx.GeneratedTypes)
            {
                StringBuilder sb = new();
                const string suffix = "GeneratedExtensions";
                string targetTypeFullName = $"{targetType.Type}";
                string targetTypeName = targetType.Type!.Name;
                var extensionClass = context.Compilation.GetTypeByMetadataName($"XamlTest.{targetTypeName}{suffix}");
                if (extensionClass is null) continue;

                string variableTargetTypeName = 
                    char.ToLowerInvariant(targetType.Type.Name[0]) 
                    + targetType.Type.Name.Substring(1);

                sb.AppendLine($@"
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;

namespace XamlTest.Tests.Generated
{{
    [TestClass]
    public class {targetType.Type.Name}{suffix}Tests
    {{
        public TestContext TestContext {{ get; set; }} = null!;

        [NotNull]
        private IApp? App {{ get; set; }}

        [TestInitialize]
        public async Task TestInitialize()
        {{
            App = XamlTest.App.StartRemote(logMessage: msg => TestContext.WriteLine(msg));

            await App.InitializeWithDefaults(Assembly.GetExecutingAssembly().Location);
        }}

        [TestCleanup]
        public void TestCleanup()
        {{
            App.Dispose();
        }}");
                foreach (IMethodSymbol getMethod in extensionClass!.GetMembers()
                    .OfType<IMethodSymbol>()
                    .Where(x => x.Name.StartsWith("Get") && x.IsStatic))
                {
                    string methodReturnType = ((INamedTypeSymbol)getMethod.ReturnType).TypeArguments[0].ToString();//.GenericTypeArguments[0].FullName;

                    sb.AppendLine($@"
        [TestMethod]
        public async Task CanInvoke_{getMethod.Name}_ReturnsValue()
        {{
            // Arrange
            await using TestRecorder recorder = new(App);

            IWindow appWindow = await App.CreateWindowWithContent(@$""<{targetTypeName} x:Name=""""Test{targetTypeName}"""" /> "");

            //Act
            IVisualElement<{targetTypeFullName}> {variableTargetTypeName} = await appWindow.GetElement<{targetTypeFullName}>(""Test{targetTypeName}"");
            var actual = await {variableTargetTypeName}.{getMethod.Name}();

            //Assert
            //{GetAssertion(getMethod.Name.Substring(3), methodReturnType)}

            recorder.Success();
        }}
");
                }


                    sb.AppendLine($@"
    }}
}}");

                context.AddSource($"{targetType.Type.Name}{suffix}Tests.cs", sb.ToString());
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        private static string GetAssertion(string propertyName, string returnType)
        {
            switch(propertyName)
            {
                case "ActualHeight":
                case "ActualWidth":
                    return "Assert.IsTrue(actual > 0);";
                case "Width":
                case "Height":
                    return "Assert.IsTrue(double.IsNaN(actual) || actual >= 0);";
                case "VerticalAlignment":
                case "HorizontalAlignment":
                    
                default:
                    return $"Assert.AreEqual(default({returnType}), actual);";
            }
        }

        public class SyntaxReceiver : ISyntaxContextReceiver
        {
            public List<TypeInfo> GeneratedTypes { get; } = new();

            public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
            {
                if (context.Node is AttributeSyntax attrib
                    && attrib.ArgumentList?.Arguments.Count >= 1
                    && context.SemanticModel.GetTypeInfo(attrib).Type?.Name == "GenerateHelpersAttribute")
                {
                    TypeOfExpressionSyntax typeArgument = (TypeOfExpressionSyntax)attrib.ArgumentList.Arguments[0].Expression;
                    TypeInfo info = context.SemanticModel.GetTypeInfo(typeArgument.Type);
                    if (info.Type is null) return;

                    GeneratedTypes.Add(info);
                }
            }
        }
    }
}
