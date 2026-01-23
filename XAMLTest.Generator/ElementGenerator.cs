using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace XAMLTest.Generator;

[Generator(LanguageNames.CSharp)]
public class ElementGenerator : IIncrementalGenerator
{
    private static DiagnosticDescriptor DuplicateAttributesWarning { get; }
        = new(id: "XAMLTEST0001",
              title: "Duplicate GenerateHelpersAttributes",
              messageFormat: "Duplicate GenerateHelpersAttributes defined for elemen type '{0}'.",
              category: "XAMLTest",
              DiagnosticSeverity.Warning,
              isEnabledByDefault: true);

    private static Dictionary<string, string> TypeRemap { get; } = new()
    {
        { "System.Windows.Controls.ColumnDefinitionCollection", "System.Collections.Generic.IList<System.Windows.Controls.ColumnDefinition>" },
        { "System.Windows.Controls.RowDefinitionCollection", "System.Collections.Generic.IList<System.Windows.Controls.RowDefinition>" }
    };

    private static HashSet<string> IgnoredTypes { get; } =
    [
        "System.Windows.TriggerCollection",
        "System.Windows.Media.CacheMode",
        "System.Windows.Input.CommandBindingCollection",
        "System.Windows.Media.Effects.Effect",
        "System.Windows.Input.InputBindingCollection",
        "System.Collections.Generic.IEnumerable<System.Windows.Input.TouchDevice>",
        "System.Windows.DependencyObjectType",
        "System.Windows.Threading.Dispatcher",
        "System.Windows.TextDecorationCollection",
        "System.Windows.Media.TextEffectCollection",
        "System.Windows.Data.BindingGroup",
        "System.Windows.Style",
        "System.Windows.ResourceDictionary",
        "System.Windows.DataTemplate",
        "System.Windows.Controls.DataTemplateSelector",
        "System.Windows.Controls.ControlTemplate",
        "System.Windows.Controls.CalendarBlackoutDatesCollection",
        "System.Windows.Controls.SelectedDatesCollection",
        "System.Windows.Controls.UIElementCollection",
        "System.Collections.ObjectModel.ObservableCollection<System.Windows.Controls.GroupStyle>",
        "System.Windows.Controls.GroupStyleSelector",
        "System.Windows.Controls.ItemContainerGenerator",
        "System.Windows.Controls.StyleSelector",
        "System.Windows.Controls.ItemCollection",
        "System.Windows.Controls.ItemsPanelTemplate",
        "System.Collections.ObjectModel.ObservableCollection<System.Windows.Controls.DataGridColumn>",
        "System.Collections.ObjectModel.ObservableCollection<System.Windows.Controls.ValidationRule>",
        "System.Collections.Generic.IList<System.Windows.Controls.DataGridCellInfo>",
        "System.Collections.IList",
        "System.Windows.Controls.Primitives.IItemContainerGenerator",
        "System.Windows.Documents.IDocumentPaginatorSource",
        "System.Windows.Documents.Typography",
        "System.Windows.Documents.InlineCollection",
        "System.Windows.Documents.TextPointer",
        "System.Windows.Controls.ItemContainerTemplateSelector",
        "System.Windows.Controls.DataGridCellInfo",
        "System.Windows.Documents.DocumentPage",
        "System.Windows.Documents.DocumentPaginator",
        "System.Windows.Documents.TextSelection",
        "System.Windows.Navigation.NavigationService",
        "System.Windows.Ink.DrawingAttributes",
        "System.Windows.Input.StylusPointDescription",
        "System.Windows.Ink.StylusShape",
        "System.Collections.Generic.IEnumerable<System.Windows.Controls.InkCanvasClipboardFormat>",
        "System.Windows.Media.MediaClock",
        "System.Windows.IInputElement",
        "System.Collections.ObjectModel.Collection<System.Windows.Controls.ToolBar>",
        "System.Windows.WindowCollection"
    ];

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
#if DEBUG
        if (!System.Diagnostics.Debugger.IsAttached)
        {
            //Debugger.Launch();
        }
#endif 

        IncrementalValuesProvider<(IReadOnlyList<VisualElement> visualElementsProvider, ImmutableArray<Diagnostic> diagnostics)> visualElementsProvider = context.SyntaxProvider.ForAttributeWithMetadataName("XamlTest.GenerateHelpersAttribute", IsGenerateHelpersAttribute, GetVisualElements);

        //context.SyntaxProvider.CreateSyntaxProvider
        //(
        //    predicate: static (node, token) => IsGenerateHelpersAttribute(node, token),
        //    transform: static (context, token) => GetVisualElements(context, token)
        //);

        // Collect all results from all attribute invocations to deduplicate across entire compilation
        IncrementalValueProvider<ImmutableArray<(IReadOnlyList<VisualElement> visualElements, ImmutableArray<Diagnostic> diagnostics)>> collectedProvider = visualElementsProvider.Collect();

        // Only generate source if enabled and DependencyInjection is referenced
        context.RegisterSourceOutput(collectedProvider, static (context, providers) =>
        {
            foreach (var diagnostic in providers.SelectMany(x => x.diagnostics).Distinct())
            {
                context.ReportDiagnostic(diagnostic);
            }

            foreach (var visualElement in providers
                .SelectMany(x => x.visualElements)
                .GroupBy(x => x.Type)
                .Select(x => x.First()))
            {
                string elementClass = GetElementContent(visualElement);
                string fileName = $"XamlTest{visualElement.Type.Name}GeneratedExtensions.g.cs";
                //System.IO.File.WriteAllText(@"D:\Dev\XAMLTest\XAMLTest\obj\" + fileName, elementClass);
                context.AddSource(fileName, elementClass);
            }
        });
    }

    private static bool IsGenerateHelpersAttribute(SyntaxNode node, CancellationToken token)
    {
        //NB: GenerateHelpersAttribute is only available at the assembly level
        return node is CompilationUnitSyntax compilation &&
                compilation.AttributeLists
                .SelectMany(x => x.Attributes)
                .Any(x => x.Name.ToFullString() == "GenerateHelpers");
    }

    private static (IReadOnlyList<VisualElement> visualElements, ImmutableArray<Diagnostic> diagnostics) GetVisualElements(GeneratorAttributeSyntaxContext context, CancellationToken token)
    {
        List<VisualElement> elements = [];

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
                //TypeOfExpressionSyntax typeArgument = (TypeOfExpressionSyntax)attrib.ArgumentList.Arguments[0].Expression;
                //TypeInfo info = context.SemanticModel.GetTypeInfo(typeArgument.Type);
                //if (info.Type is null) return;

                string? targetNamespace = null;
                foreach ((string argumentName, TypedConstant argumentType) in attribute.NamedArguments)
                {
                    switch (argumentName)
                    {
                        case "Namespace":
                            targetNamespace = argumentType.Value?.ToString();
                            //switch (argumentExpression.Expression)
                            //{
                            //    case LiteralExpressionSyntax les:
                            //        targetNamespace = les.Token.Value?.ToString();
                            //        break;
                            //    case MemberAccessExpressionSyntax maes:
                            //        targetNamespace = context.SemanticModel.GetConstantValue(maes.Name).Value?.ToString();
                            //        break;
                            //}
                            break;
                    }
                }

                string @namespace = targetNamespace ?? "XamlTest";

                for (ITypeSymbol? type = typeConstant.OriginalDefinition;
                    type is not null;
                    type = type.BaseType)
                {
                    string fullName = $"{type}";
                    if (IgnoredTypes.Contains(fullName)) continue;

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

    private static string GetElementContent(VisualElement visualElement)
    {
        StringBuilder builder = new();
        builder.AppendLine("#nullable enable");
        builder.AppendLine($"namespace {visualElement.Namespace}");
        builder.AppendLine("{");
        builder.AppendLine($"    public static partial class {GetClassName(visualElement.Type.Name)}");
        builder.AppendLine("    {");
        foreach (var property in visualElement.DependencyProperties)
        {
            builder.AppendLine();
            if (property.CanRead)
            {
                builder
                    .Append("        ");

                if (visualElement.Type.IsFinal)
                {
                    builder.AppendLine($"public static async System.Threading.Tasks.Task<{property.TypeFullName}> Get{property.Name}(this IVisualElement<{visualElement.Type.FullName}> element)");
                }
                else
                {
                    builder.AppendLine($"public static async System.Threading.Tasks.Task<{property.TypeFullName}> Get{property.Name}<T>(this IVisualElement<T> element) where T : {visualElement.Type.FullName}");
                }
                builder.Append("            ")
                    .AppendLine($"=> await element.GetProperty<{property.TypeFullName}>(nameof({visualElement.Type.FullName}.{property.Name}));");

                if (property.TypeFullName.StartsWith("System.Windows.Media.SolidColorBrush") ||
                    property.TypeFullName.StartsWith("System.Windows.Media.Brush"))
                {
                    builder
                    .Append("        ");
                    if (visualElement.Type.IsFinal)
                    {
                        builder.AppendLine($"public static async System.Threading.Tasks.Task<System.Windows.Media.Color?> Get{property.Name}Color(this IVisualElement<{visualElement.Type.FullName}> element)");
                    }
                    else
                    {
                        builder.AppendLine($"public static async System.Threading.Tasks.Task<System.Windows.Media.Color?> Get{property.Name}Color<T>(this IVisualElement<T> element) where T : {visualElement.Type.FullName}");
                    }
                    builder
                    .Append("            ")
                    .AppendLine($"=> await element.GetProperty<System.Windows.Media.Color?>(nameof({visualElement.Type.FullName}.{property.Name}));");
                }
            }
            if (property.CanWrite)
            {
                builder
                    .Append("        ");

                if (visualElement.Type.IsFinal)
                {
                    builder.AppendLine($"public static async System.Threading.Tasks.Task<{property.TypeFullName}> Set{property.Name}(this IVisualElement<{visualElement.Type.FullName}> element, {property.TypeFullName} value)");
                }
                else
                {
                    builder.AppendLine($"public static async System.Threading.Tasks.Task<{property.TypeFullName}> Set{property.Name}<T>(this IVisualElement<T> element, {property.TypeFullName} value) where T : {visualElement.Type.FullName}");
                }
                builder
                    .Append("            ")
                    .AppendLine($"=> await element.SetProperty(nameof({visualElement.Type.FullName}.{property.Name}), value);");

                if (property.TypeFullName.StartsWith("System.Windows.Media.SolidColorBrush") ||
                    property.TypeFullName.StartsWith("System.Windows.Media.Brush"))
                {
                    builder
                    .Append("        ");
                    if (visualElement.Type.IsFinal)
                    {
                        builder.AppendLine($"public static async System.Threading.Tasks.Task<System.Windows.Media.Color?> Set{property.Name}Color(this IVisualElement<{visualElement.Type.FullName}> element, System.Windows.Media.Color value)");
                    }
                    else
                    {
                        builder.AppendLine($"public static async System.Threading.Tasks.Task<System.Windows.Media.Color?> Set{property.Name}Color<T>(this IVisualElement<T> element, System.Windows.Media.Color value) where T : {visualElement.Type.FullName}");
                    }
                    builder
                    .AppendLine("        {")
                    .Append("            ")
                    .AppendLine($"System.Windows.Media.SolidColorBrush? brush = await element.SetProperty(nameof({visualElement.Type.FullName}.{property.Name}), new System.Windows.Media.SolidColorBrush(value));")
                    .Append("            ")
                    .AppendLine("return brush?.Color ?? default;")
                    .AppendLine("        }");
                }

            }
        }
        builder.AppendLine("    }");
        builder.AppendLine("}");

        return builder.ToString();
    }

    private static string GetClassName(string typeName) 
        => $"{typeName}GeneratedExtensions";

    private static string GetSafeTypeName(ITypeSymbol typeSymbol)
    {
        string safeTypeName = typeSymbol.Name;

        if (typeSymbol is INamedTypeSymbol { TypeArguments.Length: > 0 } genericSymbol)
        {
            safeTypeName += $"_{string.Join("_", genericSymbol.TypeArguments.Select(x => GetSafeTypeName(x)))}";
        }
        return safeTypeName;
    }

    private static bool ShouldUseVisualElement(ITypeSymbol typeSymbol)
    {
        for (ITypeSymbol? type = typeSymbol;
             type != null;
             type = type.BaseType)
        {
            switch ($"{type}")
            {
                case "System.Windows.Media.Brush": return false;
                case "System.Windows.DependencyObject": return true;
            }
        }
        return false;
    }

    private static bool IsDelegate(ITypeSymbol typeSymbol)
       => Is(typeSymbol, "System.Delegate");

    private static bool Is(ITypeSymbol typeSymbol, string targetType)
    {
        for (ITypeSymbol? type = typeSymbol;
            type != null;
            type = type.BaseType)
        {
            if ($"{type}" == targetType)
            {
                return true;
            }
        }
        return false;
    }
}
