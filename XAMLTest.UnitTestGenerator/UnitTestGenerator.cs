using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TypeInfo = Microsoft.CodeAnalysis.TypeInfo;

namespace XAMLTest.UnitTestGenerator;

[Generator]
public class UnitTestGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Create a provider for GenerateTestsAttribute syntax nodes
        var attributeProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null);

        // Combine with compilation to access type information
        var compilationAndTypes = context.CompilationProvider.Combine(attributeProvider.Collect());
        
        context.RegisterSourceOutput(compilationAndTypes, static (spc, source) => Execute(source.Left, source.Right, spc));
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        return node is AttributeSyntax attrib 
            && attrib.ArgumentList?.Arguments.Count >= 1;
    }

    private static TypeInfo? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var attrib = (AttributeSyntax)context.Node;
        
        var typeInfo = context.SemanticModel.GetTypeInfo(attrib);
        if (typeInfo.Type?.Name != "GenerateTestsAttribute")
            return null;

        if (attrib.ArgumentList?.Arguments.Count < 1)
            return null;

        var typeArgument = (TypeOfExpressionSyntax)attrib.ArgumentList!.Arguments[0].Expression;
        var info = context.SemanticModel.GetTypeInfo(typeArgument.Type);
        if (info.Type is null) 
            return null;

        return info;
    }

    private static void Execute(Compilation compilation, ImmutableArray<TypeInfo?> types, SourceProductionContext context)
    {
#if DEBUG
        if (!Debugger.IsAttached)
        {
            //Debugger.Launch();
        }
#endif
        
        if (types.IsDefaultOrEmpty)
            return;

        var validTypes = types.Where(x => x is not null).Cast<TypeInfo>().ToList();
        
        foreach (TypeInfo targetType in validTypes)
        {
            if (targetType.Type?.IsAbstract == true) continue;

            StringBuilder sb = new();
            const string suffix = "GeneratedExtensions";
            string targetTypeFullName = $"{targetType.Type}";
            string targetTypeName = targetType.Type!.Name;
            var extensionClass = compilation.GetTypeByMetadataName($"XamlTest.{targetTypeName}{suffix}");
            if (extensionClass is null) continue;

            string variableTargetTypeName = 
                char.ToLowerInvariant(targetType.Type.Name[0]) 
                + targetType.Type.Name.Substring(1);

            string className = $"{targetType.Type.Name}{suffix}Tests";

            sb.AppendLine($@"
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using System;

namespace XamlTest.Tests.Generated
{{
    [TestClass]
    public partial class {className}
    {{
        [NotNull]
        private static IApp? App {{ get; set; }}

        [NotNull]
        private static IWindow? Window {{ get; set; }}

        private static Func<string, string> GetWindowContent {{ get; set; }} = x => x;

        private static Func<string, Task<IVisualElement<{targetTypeFullName}>>> GetElement {{ get; set; }}
            = x => Window.GetElement<{targetTypeFullName}>(x);

        static partial void OnClassInitialize();

        [ClassInitialize]
        public static async Task ClassInitialize(TestContext context)
        {{
            OnClassInitialize();
            App = await XamlTest.App.StartRemote(logMessage: msg => context.WriteLine(msg));

            await App.InitializeWithDefaults(Assembly.GetExecutingAssembly().Location);

            string content = @$""<{targetTypeName} x:Name=""""Test{targetTypeName}""""/>"";

            content = GetWindowContent(content);

            Window = await App.CreateWindowWithContent(content);
        }}

        [ClassCleanup(ClassCleanupBehavior.EndOfClass)]
        public static async Task TestCleanup()
        {{
            if (App is {{ }} app)
            {{
                await app.DisposeAsync();
                App = null;
            }}
        }}

");

            foreach ((IMethodSymbol getMethod, IFieldSymbol dependencyProperty) in GetTestMethods(extensionClass))
            {
                string methodReturnType = ((INamedTypeSymbol)getMethod.ReturnType).TypeArguments[0].ToString();

                sb.AppendLine($@"
        [TestMethod]
        public async Task CanInvoke_{getMethod.Name}_ReturnsValue()
        {{
            // Arrange
            await using TestRecorder recorder = new(App);

            //Act
            IVisualElement<{targetTypeFullName}> {variableTargetTypeName} = await GetElement(""Test{targetTypeName}"");
            var actual = await {variableTargetTypeName}.{getMethod.Name}();

            //Assert
            /*
            {GetAssertion(getMethod.Name.Substring(3), methodReturnType, dependencyProperty)}
            */

            recorder.Success();
        }}
");
            }


                sb.AppendLine($@"
    }}
}}");

            //System.IO.File.WriteAllText($@"D:\Dev\XAMLTest\XAMLTest.UnitTestGenerator\obj\{className}.cs", sb.ToString());

            context.AddSource($"{className}.cs", sb.ToString());
        }
    }

    static IEnumerable<(IMethodSymbol, IFieldSymbol)> GetTestMethods(INamedTypeSymbol extensionClass)
        {
            for (INamedTypeSymbol? type = extensionClass;
                type != null;
                type = type.BaseType)
            {
                //Pick up the abstract base stuff
                if (type.IsAbstract) continue;
                foreach (IMethodSymbol getMethod in type.GetMembers()
                    .OfType<IMethodSymbol>()
                    .Where(x => x.Name.StartsWith("Get") && x.IsStatic))
                {
                    IFieldSymbol dependencyProperty = 
                        type.GetMembers()
                            .OfType<IFieldSymbol>()
                            .Where(x => x.Name == getMethod.Name.Substring(3) + "Property")
                            .FirstOrDefault();
                    yield return (getMethod, dependencyProperty);
                }
            }
    }

    static string GetAssertion(string propertyName, string returnType, IFieldSymbol dependencyProperty)
        {
            return propertyName switch
            {
                "ActualHeight" or "ActualWidth" => "Assert.IsTrue(actual > 0);",
                "Width" or "Height" => "Assert.IsTrue(double.IsNaN(actual) || actual >= 0);",
                _ when dependencyProperty is not null => $"""
                    object expected = {dependencyProperty.ContainingNamespace}.{dependencyProperty.ContainingType.Name}.{dependencyProperty.Name}.DefaultMetadata.DefaultValue;
                    Assert.AreEqual(expected, actual);
                    """,
                _ => $"Assert.AreEqual(default({returnType}), actual);",
        };
    }
}
