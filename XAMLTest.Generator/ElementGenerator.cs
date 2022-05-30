using Microsoft.CodeAnalysis;
using System.Text;

namespace XAMLTest.Generator;

[Generator]
public class ElementGenerator : ISourceGenerator
{
    private static DiagnosticDescriptor DuplicateAttributesWarning { get; }
        = new(id: "XAMLTEST0001",
              title: "Duplicate GenerateHelpersAttributes",
              messageFormat: "Duplicate GenerateHelpersAttributes defined for elemen type '{0}'.",
              category: "XAMLTest",
              DiagnosticSeverity.Warning,
              isEnabledByDefault: true);

    public void Execute(GeneratorExecutionContext context)
    {
        SyntaxReceiver rx = (SyntaxReceiver)context.SyntaxContextReceiver!;
        HashSet<string> ignoredTypes = new();
        foreach (var duplicatedAttributes in rx.GeneratedTypes.GroupBy(x => x.Type.FullName).Where(g => g.Count() > 1))
        {
            context.ReportDiagnostic(Diagnostic.Create(DuplicateAttributesWarning, Location.None, duplicatedAttributes.Key));
            ignoredTypes.Add(duplicatedAttributes.Key);
            return;
        }

        foreach (var type in rx.GeneratedTypes)
        {
            if (ignoredTypes.Contains(type.Type.FullName)) continue;

            if (context.Compilation.GetTypeByMetadataName($"{type.Namespace}.{type.Type.Name}GeneratedExtensions") is not null)
            {
                continue;
            }

            StringBuilder builder = new();
            builder.AppendLine("#nullable enable");
            builder.AppendLine($"namespace {type.Namespace}");
            builder.AppendLine("{");
            builder.AppendLine($"    public static partial class {type.Type.Name}GeneratedExtensions");
            builder.AppendLine("    {");
            foreach (var property in type.DependencyProperties)
            {
                builder.AppendLine();
                if (property.CanRead)
                {
                    builder
                        .Append("        ");

                    if (type.Type.IsFinal)
                    {
                        builder.AppendLine($"public static async System.Threading.Tasks.Task<{property.TypeFullName}> Get{property.Name}(this IVisualElement<{type.Type.FullName}> element)");
                    }
                    else
                    {
                        builder.AppendLine($"public static async System.Threading.Tasks.Task<{property.TypeFullName}> Get{property.Name}<T>(this IVisualElement<T> element) where T : {type.Type.FullName}");
                    }
                    builder.Append("            ")
                        .AppendLine($"=> await element.GetProperty<{property.TypeFullName}>(nameof({type.Type.FullName}.{property.Name}));");

                    if (IsBrushType(property))
                    {
                        string colorType = GetColorType(property);
                        builder.Append("        ");
                        if (type.Type.IsFinal)
                        {
                            builder.AppendLine($"public static async System.Threading.Tasks.Task<{colorType}?> Get{property.Name}Color(this IVisualElement<{type.Type.FullName}> element)");
                        }
                        else
                        {
                            builder.AppendLine($"public static async System.Threading.Tasks.Task<{colorType}?> Get{property.Name}Color<T>(this IVisualElement<T> element) where T : {type.Type.FullName}");
                        }
                        builder
                        .Append("            ")
                        .AppendLine($"=> await element.GetProperty<{colorType}?>(nameof({type.Type.FullName}.{property.Name}));");
                    }
                }
                if (property.CanWrite)
                {
                    builder.Append("        ");

                    if (type.Type.IsFinal)
                    {
                        builder.AppendLine($"public static async System.Threading.Tasks.Task<{property.TypeFullName}> Set{property.Name}(this IVisualElement<{type.Type.FullName}> element, {property.TypeFullName} value)");
                    }
                    else
                    {
                        builder.AppendLine($"public static async System.Threading.Tasks.Task<{property.TypeFullName}> Set{property.Name}<T>(this IVisualElement<T> element, {property.TypeFullName} value) where T : {type.Type.FullName}");
                    }
                    builder
                        .Append("            ")
                        .AppendLine($"=> await element.SetProperty(nameof({type.Type.FullName}.{property.Name}), value);");

                    if (IsBrushType(property))
                    {
                        string colorType = GetColorType(property);
                        string solidColorBrushType = GetSolidColorBrushType(property);
                        builder.Append("        ");
                        if (type.Type.IsFinal)
                        {
                            builder.AppendLine($"public static async System.Threading.Tasks.Task<{colorType}?> Set{property.Name}Color(this IVisualElement<{type.Type.FullName}> element, {colorType} value)");
                        }
                        else
                        {
                            builder.AppendLine($"public static async System.Threading.Tasks.Task<{colorType}?> Set{property.Name}Color<T>(this IVisualElement<T> element, {colorType} value) where T : {type.Type.FullName}");
                        }
                        builder
                        .AppendLine("        {")
                        .Append("            ")
                        .AppendLine($"{solidColorBrushType}? brush = await element.SetProperty(nameof({type.Type.FullName}.{property.Name}), new {solidColorBrushType}(value));")
                        .Append("            ")
                        .AppendLine("return brush?.Color ?? default;")
                        .AppendLine("        }");
                    }

                }
            }
            builder.AppendLine("    }");
            builder.AppendLine("}");

            string fileName = $"XamlTest{type.Type.Name}GeneratedExtensions.g.cs";
            //System.IO.File.WriteAllText(@"D:\Dev\XAMLTest\XAMLTest\obj\" + fileName, builder.ToString());
            context.AddSource(fileName, builder.ToString());
        }
    }

    public void Initialize(GeneratorInitializationContext context)
    {
#if DEBUG
        if (!System.Diagnostics.Debugger.IsAttached)
        {
            //System.Diagnostics.Debugger.Launch();
        }
#endif 
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    private static bool IsBrushType(Property property) => IsBrush(property.TypeFullName);

    private static bool IsBrush(string fullTypeName)
    {
        return fullTypeName.StartsWith("System.Windows.Media.SolidColorBrush") ||
               fullTypeName.StartsWith("System.Windows.Media.Brush") ||
               fullTypeName.StartsWith("Microsoft.UI.Xaml.Media.Brush");
    }

    private static string GetColorType(string brushFullTypeName)
    {
        if (brushFullTypeName.StartsWith("System.Windows.Media"))
        {
            return "System.Windows.Media.Color";
        }
        return "Windows.UI.Color";
    }

    private static string GetColorType(Property property) => GetColorType(property.TypeFullName);

    private static string GetSolidColorBrushType(string brushFullTypeName)
    {
        if (brushFullTypeName.StartsWith("System.Windows.Media"))
        {
            return "System.Windows.Media.SolidColorBrush";
        }
        return "Microsoft.UI.Xaml.Media.SolidColorBrush";
    }

    private static string GetSolidColorBrushType(Property property) => GetSolidColorBrushType(property.TypeFullName);
}
