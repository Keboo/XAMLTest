using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using TypeInfo = Microsoft.CodeAnalysis.TypeInfo;

namespace XAMLTest.UnitTestGenerator;

[Generator(LanguageNames.CSharp)]
public partial class UnitTestGenerator : IIncrementalGenerator
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
            if (targetType.Type?.IsAbstract == true) continue;

            StringBuilder sb = new();
            const string suffix = "GeneratedExtensions";
            string targetTypeFullName = $"{targetType.Type}";
            string targetTypeName = targetType.Type!.Name;
            var extensionClass = context.Compilation.GetTypeByMetadataName($"XamlTest.{targetTypeName}{suffix}");
            if (extensionClass is null) continue;

            string variableTargetTypeName =
                char.ToLowerInvariant(targetType.Type.Name[0])
                + targetType.Type.Name.Substring(1);

            string className = $"{targetType.Type.Name}{suffix}Tests";

            sb.AppendLine($$"""
                using Microsoft.VisualStudio.TestTools.UnitTesting;
                using System.Diagnostics.CodeAnalysis;
                using System.Reflection;
                using System.Threading.Tasks;
                using System;

                namespace XamlTest.Tests.Generated
                {
                    [TestClass]
                    public partial class {{className}}
                    {
                        [NotNull]
                        private static IApp? App { get; set; }

                        [NotNull]
                        private static IWindow? Window { get; set; }

                        private static Func<string, string> GetWindowContent { get; set; } = x => x;

                        private static Func<string, Task<IVisualElement<{{targetTypeFullName}}>>> GetElement { get; set; }
                            = x => Window.GetElement<{{targetTypeFullName}}>(x);

                        static partial void OnClassInitialize();

                        [ClassInitialize]
                        public static async Task ClassInitialize(TestContext context)
                        {
                            OnClassInitialize();
                            App = await XamlTest.App.StartRemote(logMessage: msg => context.WriteLine(msg));

                            await App.InitializeWithDefaults(Assembly.GetExecutingAssembly().Location);

                            string content = @$"<{{targetTypeName}} x:Name=""Test{{targetTypeName}}""/>";

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

            foreach ((IMethodSymbol getMethod, IFieldSymbol dependencyProperty) in GetTestMethods(extensionClass))
            {
                string methodReturnType = ((INamedTypeSymbol)getMethod.ReturnType).TypeArguments[0].ToString();

                sb.AppendLine($$"""

                            [TestMethod]
                            public async Task CanInvoke_{{getMethod.Name}}_ReturnsValue()
                            {
                                // Arrange
                                await using TestRecorder recorder = new(App);

                                //Act
                                IVisualElement<{{targetTypeFullName}}> {{variableTargetTypeName}} = await GetElement("Test{{targetTypeName}}");
                                var actual = await {{variableTargetTypeName}}.{{getMethod.Name}}();

                                //Assert
                                /*
                                {{GetAssertion(getMethod.Name.Substring(3), methodReturnType, dependencyProperty)}}
                                */

                                recorder.Success();
                            }

                    """);
            }


            sb.AppendLine($$"""

                    }
                }
                """);

            //System.IO.File.WriteAllText($@"D:\Dev\XAMLTest\XAMLTest.UnitTestGenerator\obj\{className}.cs", sb.ToString());

            context.AddSource($"{className}.cs", sb.ToString());
        }

        static IEnumerable<(IMethodSymbol, IFieldSymbol)> GetTestMethods(INamedTypeSymbol extensionClass)
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

    public void Initialize(GeneratorInitializationContext context)
    {
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<(IReadOnlyList<TypeInfo> visualElementsProvider, ImmutableArray<Diagnostic> diagnostics)> visualElementsProvider = context.SyntaxProvider.ForAttributeWithMetadataName("XamlTest.GenerateHelpersAttribute", IsGenerateHelpersAttribute, GetVisualElements);

    }

    private static bool IsGenerateHelpersAttribute(SyntaxNode node, CancellationToken token)
    {
        //NB: GenerateHelpersAttribute is only available at the assembly level
        return node is CompilationUnitSyntax compilation &&
                compilation.AttributeLists
                .SelectMany(x => x.Attributes)
                .Any(x => x.Name.ToFullString() == "GenerateHelpers");
    }

    private static (IReadOnlyList<TypeInfo> generatedElements, ImmutableArray<Diagnostic> diagnostics) GetVisualElements(GeneratorAttributeSyntaxContext context, CancellationToken token)
    {
        List<GeneratedElement> elements = [];

        foreach (AttributeData attribute in context.Attributes)
        {
            if (attribute.AttributeClass?.Name != "GenerateHelpersAttribute")
            {
                continue;
            }

            if (attribute.ConstructorArguments[0].Kind != TypedConstantKind.Type)
            {
                //TODO Diagnostic
            }



            if (attribute.ConstructorArguments is { Length: 1 } &&
                attribute.ConstructorArguments[0].Value is INamedTypeSymbol typeConstant)
            {
                //if (context.SemanticModel.Compilation.GetTypeByMetadataName($"{typeConstant}") is { } TypeInfo info)
                //{ 
                //}
                
                if (info.Type is null) return;

                string @namespace = "XamlTest";

                for (ITypeSymbol? type = typeConstant.OriginalDefinition;
                    type is not null;
                    type = type.BaseType)
                {
                    string fullName = $"{type}";
                  
                    string safeTypeName = GetSafeTypeName(type);

                    if (context.SemanticModel.Compilation.GetTypeByMetadataName($"{@namespace}.{GetClassName(safeTypeName)}") is not null) continue;

                    List<Property> properties = [];

                    if (elements.Any(x => x.Type.FullName == fullName)) continue;

                    foreach (ISymbol member in type.GetMembers())
                    {
                        if (member is IPropertySymbol property &&
                            property.CanBeReferencedByName &&
                            !property.IsStatic &&
                            !property.IsOverride &&
                            property.DeclaredAccessibility == Accessibility.Public &&
                            !property.GetAttributes()
                                .Any(x => x.AttributeClass?.Name == "ObsoleteAttribute" || x.AttributeClass?.Name == "ExperimentalAttribute") &&
                            !IgnoredTypes.Contains($"{property.Type}") &&
                            !IsDelegate(property.Type))
                        {
                            if (ShouldUseVisualElement(property.Type))
                            {
                                properties.Add(
                                    new Property(
                                        property.Name,
                                        $"XamlTest.IVisualElement<{property.Type}>?",
                                        property.GetMethod is not null,
                                        property.SetMethod is not null));
                            }
                            else
                            {
                                string propertyType = $"{property.Type}";
                                if (TypeRemap.TryGetValue(propertyType, out string? remappedType))
                                {
                                    propertyType = remappedType;
                                }

                                if (property.Type.IsReferenceType &&
                                    !propertyType.EndsWith("?"))
                                {
                                    propertyType += "?";
                                }

                                properties.Add(
                                    new Property(
                                        property.Name,
                                        propertyType,
                                        property.GetMethod is not null,
                                        property.SetMethod is not null));
                            }
                        }
                    }
                    if (properties.Any())
                    {
                        var visualElementType = new VisualElementType(safeTypeName, fullName, type.IsSealed || type.IsValueType);
                        elements.Add(new VisualElement(@namespace, visualElementType, properties));
                    }
                }
            }
        }
        return (elements, ImmutableArray<Diagnostic>.Empty);
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



public record class GeneratedElement(string Name, string TypeName);
