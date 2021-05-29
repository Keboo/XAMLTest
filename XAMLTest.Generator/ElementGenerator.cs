using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace XAMLTest.Generator
{
    [Generator]
    public class ElementGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            SyntaxReceiver rx = (SyntaxReceiver)context.SyntaxContextReceiver!;
            foreach (var type in rx.GeneratedTypes)
            {
                StringBuilder builder = new();
                builder.AppendLine($"namespace {type.Namespace}");
                builder.AppendLine("{");
                builder.AppendLine($"    public static partial class {type.Type.Name}GeneratedExtensions");
                builder.AppendLine("    {");
                foreach (var property in type.DependencyProperties)
                {
                    /*
                     * public static async Task<double> GetActualHeight(this IVisualElement element)
                     */
                    builder.AppendLine();
                    if (property.CanRead)
                    {
                        builder
                            .Append("        ")
                            .AppendLine($"public static async System.Threading.Tasks.Task<{property.TypeFullName}> Get{property.Name}(this IVisualElement<{type.Type.FullName}> element)")
                            .Append("            ")
                            .AppendLine($"=> await element.GetProperty<{property.TypeFullName}>(nameof({type.Type.FullName}.{property.Name}));");
                    }
                    if (property.CanWrite)
                    {
                        builder
                            .Append("        ")
                            .AppendLine($"public static async System.Threading.Tasks.Task<{property.TypeFullName}> Set{property.Name}(this IVisualElement<{type.Type.FullName}> element, {property.TypeFullName} value)")
                            .Append("            ")
                            .AppendLine($"=> await element.SetProperty(nameof({type.Type.FullName}.{property.Name}), value);");
                    }
                }
                builder.AppendLine("    }");
                builder.AppendLine("}");

                string fileName = $"XamlTest{type.Type.Name}GeneratedExtensions.g.cs";
                //File.WriteAllText(@"D:\Dev\XAMLTest\XAMLTest\obj\" + fileName, builder.ToString());
                context.AddSource(fileName, builder.ToString());
            }

        }

        public void Initialize(GeneratorInitializationContext context)
        {
#if DEBUG
            if (!Debugger.IsAttached)
            {
                //Debugger.Launch();
            }
#endif 
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }
    }

    public record VisualElement(
        string Namespace,
        VisualElementType Type,
        IReadOnlyList<Property> DependencyProperties)
    { }

    public record VisualElementType(string Name, string FullName)
    { }

    public record Property(string Name, string TypeFullName, bool CanRead, bool CanWrite)
    { }

    public class SyntaxReceiver : ISyntaxContextReceiver
    {
        private List<VisualElement> Elements { get; } = new();
        public IReadOnlyList<VisualElement> GeneratedTypes => Elements;

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            // find all valid mustache attributes
            if (context.Node is AttributeSyntax attrib
                && attrib.ArgumentList?.Arguments.Count == 1
                && context.SemanticModel.GetTypeInfo(attrib).Type?.Name == "GenerateHelpersAttribute")
            {
                List<Property> properties = new();
                TypeOfExpressionSyntax typeArgument = (TypeOfExpressionSyntax)attrib.ArgumentList.Arguments[0].Expression;
                TypeInfo info = context.SemanticModel.GetTypeInfo(typeArgument.Type);
                if (info.Type is null) return;

                for (ITypeSymbol? type = info.Type;
                    type is not null;
                    type = type.BaseType)
                {
                    foreach (ISymbol member in type.GetMembers())
                    {
                        if (member is IPropertySymbol property &&
                            property.CanBeReferencedByName &&
                            property.DeclaredAccessibility == Accessibility.Public &&
                            !property.GetAttributes().Any(x => x.AttributeClass.Name == "ObsoleteAttribute"))
                        {
                            properties.Add(new Property(
                                property.Name,
                                $"{property.Type}",
                                property.GetMethod is not null,
                                property.SetMethod is not null));
                        }
                    }
                }

                var visualElementType = new VisualElementType(info.Type.Name, $"{info.Type}");
                Elements.Add(new VisualElement("XamlTest", visualElementType, properties));
            }
        }
    }
}
