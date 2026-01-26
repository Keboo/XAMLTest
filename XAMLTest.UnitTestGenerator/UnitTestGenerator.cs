using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace XAMLTest.UnitTestGenerator;

[Generator(LanguageNames.CSharp)]
public class UnitTestGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
#if DEBUG
        if (!System.Diagnostics.Debugger.IsAttached)
        {
            //System.Diagnostics.Debugger.Launch();
        }
#endif

        // Use CompilationProvider to access referenced assemblies
        IncrementalValueProvider<(IReadOnlyList<TestClass> testClasses, ImmutableArray<Diagnostic> diagnostics)> testClassProvider = 
            context.CompilationProvider.Select(static (compilation, cancellationToken) =>
            {
                return GetTestClassesFromCompilation(compilation, cancellationToken);
            });

        // Generate source for each test class
        context.RegisterSourceOutput(testClassProvider, static (context, result) =>
        {
            foreach (var diagnostic in result.diagnostics)
            {
                context.ReportDiagnostic(diagnostic);
            }

            foreach (var testClass in result.testClasses)
            {
                string testClassContent = GetTestClassContent(testClass);
                string fileName = $"{testClass.ClassName}.g.cs";
                //System.IO.File.WriteAllText(@"D:\Dev\XAMLTest\XAMLTest.UnitTestGenerator\obj\" + fileName, testClassContent);
                context.AddSource(fileName, testClassContent);
            }
        });
    }

    private static (IReadOnlyList<TestClass> testClasses, ImmutableArray<Diagnostic> diagnostics) GetTestClassesFromCompilation(
        Compilation compilation,
        CancellationToken token)
    {
        List<TestClass> testClasses = [];

        // Check current assembly attributes
        foreach (AttributeData attribute in compilation.Assembly.GetAttributes())
        {
            ProcessGenerateHelpersAttribute(attribute, compilation, testClasses);
        }

        // Check referenced assemblies for GenerateHelpersAttribute
        foreach (var reference in compilation.References)
        {
            if (compilation.GetAssemblyOrModuleSymbol(reference) is IAssemblySymbol assemblySymbol)
            {
                foreach (AttributeData attribute in assemblySymbol.GetAttributes())
                {
                    ProcessGenerateHelpersAttribute(attribute, compilation, testClasses);
                }
            }
        }

        return (testClasses, ImmutableArray<Diagnostic>.Empty);
    }

    private static void ProcessGenerateHelpersAttribute(
        AttributeData attribute,
        Compilation compilation,
        List<TestClass> testClasses)
    {
        if (attribute.AttributeClass?.Name != "GenerateHelpersAttribute")
        {
            return;
        }

        if (attribute.ConstructorArguments[0].Kind != TypedConstantKind.Type)
        {
            //TODO Diagnostic
            return;
        }

        if (attribute.ConstructorArguments is { Length: 1 } &&
            attribute.ConstructorArguments[0].Value is INamedTypeSymbol typeConstant)
        {
            string @namespace = "XamlTest";

            for (ITypeSymbol? type = typeConstant.OriginalDefinition;
                type is not null;
                type = type.BaseType)
            {
                if (type.IsAbstract) continue;

                string fullName = $"{type}";
                string safeTypeName = GetSafeTypeName(type);
                string extensionClassName = $"{safeTypeName}GeneratedExtensions";

                // Check if the extension class exists
                var extensionClass = compilation.GetTypeByMetadataName($"{@namespace}.{extensionClassName}");
                if (extensionClass is null) continue;

                // Check if we already have this test class
                if (testClasses.Any(x => x.TargetType.FullName == fullName)) continue;

                // Get all test methods from the extension class
                List<TestMethod> testMethods = [];
                foreach ((IMethodSymbol getMethod, IFieldSymbol dependencyProperty) in GetTestMethods(extensionClass))
                {
                    string methodReturnType = getMethod.ReturnType is INamedTypeSymbol returnTypeSymbol && returnTypeSymbol.IsGenericType
                        ? returnTypeSymbol.TypeArguments[0].ToString()
                        : getMethod.ReturnType.ToString();

                    string propertyName = getMethod.Name.StartsWith("Get") ? getMethod.Name.Substring(3) : getMethod.Name;
                    string assertion = GetAssertion(propertyName, methodReturnType, dependencyProperty);

                    testMethods.Add(new TestMethod(
                        getMethod.Name,
                        propertyName,
                        methodReturnType,
                        assertion));
                }

                if (testMethods.Any())
                {
                    string variableTargetTypeName =
                        char.ToLowerInvariant(type.Name[0])
                        + type.Name.Substring(1);

                    var targetType = new TargetType(type.Name, fullName);
                    string className = $"{type.Name}GeneratedExtensionsTests";

                    testClasses.Add(new TestClass(
                        className,
                        targetType,
                        variableTargetTypeName,
                        testMethods));
                }
            }
        }
    }

    private static string GetTestClassContent(TestClass testClass)
    {
        StringBuilder sb = new();

        sb.AppendLine($$"""
            #nullable enable
            using Microsoft.VisualStudio.TestTools.UnitTesting;
            using System.Diagnostics.CodeAnalysis;
            using System.Reflection;
            using System.Threading.Tasks;
            using System;

            namespace XamlTest.Tests.Generated
            {
                [TestClass]
                public partial class {{testClass.ClassName}}
                {
                    [NotNull]
                    private static IApp? App { get; set; }

                    [NotNull]
                    private static IWindow? Window { get; set; }

                    private static Func<string, string> GetWindowContent { get; set; } = x => x;

                    private static Func<string, Task<IVisualElement<{{testClass.TargetType.FullName}}>>> GetElement { get; set; }
                        = x => Window.GetElement<{{testClass.TargetType.FullName}}>(x);

                    static partial void OnClassInitialize();

                    [ClassInitialize]
                    public static async Task ClassInitialize(TestContext context)
                    {
                        OnClassInitialize();
                        App = await XamlTest.App.StartRemote(logMessage: msg => context.WriteLine(msg));

                        await App.InitializeWithDefaults(Assembly.GetExecutingAssembly().Location);

                        string content = @$"<{{testClass.TargetType.Name}} x:Name=""Test{{testClass.TargetType.Name}}""/>";

                        content = GetWindowContent(content);

                        Window = await App.CreateWindowWithContent(content);
                    }

                    [ClassCleanup(Microsoft.VisualStudio.TestTools.UnitTesting.InheritanceBehavior.BeforeEachDerivedClass)]
                    public static async Task TestCleanup()
                    {
                        if (App is { } app)
                        {
                            await app.DisposeAsync();
                            App = null;
                        }
                    }

            """);

        foreach (var testMethod in testClass.TestMethods)
        {
            sb.AppendLine($$"""

                        [TestMethod]
                        public async Task CanInvoke_{{testMethod.MethodName}}_ReturnsValue()
                        {
                            // Arrange
                            await using TestRecorder recorder = new(App);

                            //Act
                            IVisualElement<{{testClass.TargetType.FullName}}> {{testClass.VariableName}} = await GetElement("Test{{testClass.TargetType.Name}}");
                            var actual = await {{testClass.VariableName}}.{{testMethod.MethodName}}();

                            //Assert
                            /*
                            {{testMethod.Assertion}}
                            */

                            recorder.Success();
                        }

                """);
        }

        sb.AppendLine($$"""

                }
            }
            """);

        return sb.ToString();
    }

    private static IEnumerable<(IMethodSymbol, IFieldSymbol)> GetTestMethods(INamedTypeSymbol extensionClass)
    {
        for (INamedTypeSymbol? type = extensionClass;
            type != null;
            type = type.BaseType)
        {
            if (type.IsAbstract) continue;
            foreach (IMethodSymbol getMethod in type.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(x => x.Name.StartsWith("Get") && x.IsStatic))
            {
                IFieldSymbol dependencyProperty =
                    type.GetMembers()
                        .OfType<IFieldSymbol>()
                        .FirstOrDefault(x => x.Name == getMethod.Name.Substring(3) + "Property");
                yield return (getMethod, dependencyProperty);
            }
        }
    }

    private static string GetAssertion(string propertyName, string returnType, IFieldSymbol dependencyProperty)
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

    private static string GetSafeTypeName(ITypeSymbol typeSymbol)
    {
        string safeTypeName = typeSymbol.Name;

        if (typeSymbol is INamedTypeSymbol { TypeArguments.Length: > 0 } genericSymbol)
        {
            safeTypeName += $"_{string.Join("_", genericSymbol.TypeArguments.Select(x => GetSafeTypeName(x)))}";
        }
        return safeTypeName;
    }
}

// Record classes for storing test generation data
public record class TestClass(
    string ClassName,
    TargetType TargetType,
    string VariableName,
    IReadOnlyList<TestMethod> TestMethods);

public record class TargetType(
    string Name,
    string FullName);

public record class TestMethod(
    string MethodName,
    string PropertyName,
    string ReturnType,
    string Assertion);
