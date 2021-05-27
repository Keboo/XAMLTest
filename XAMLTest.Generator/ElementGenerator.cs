using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mime;
using System.Text;

namespace XAMLTest.Generator
{
    [Generator]
    public class ElementGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            SyntaxReceiver rx = (SyntaxReceiver)context.SyntaxContextReceiver!;
            foreach(var type in rx.GeneratedTypes)
            {
                StringBuilder builder = new();
                builder.AppendLine("namespace {Namespace}");
                builder.AppendLine("{");
                builder.AppendLine("    partial static void {ClassName}");
                builder.AppendLine("    {");
                foreach(var dependencyProperty in type.DependencyProperties)
                {
                    /*
                     * public static async Task<double> GetActualHeight(this IVisualElement element)
                     */
                    builder.AppendLine();
                    if (dependencyProperty.CanRead)
                    {
                        builder
                            .Append("        ")
                            .AppendLine("public static async Task<{Type}> Get{Name}(this IVisualElement<{type.FullName}> element)")
                            .AppendLine("        {")
                            .Append("            ")
                            .AppendLine("=> await element.GetProperty<{Type}>(nameof({type.FullName}.{Name}));")
                            .AppendLine("        }");
                    }
                    if (dependencyProperty.CanWrite)
                    {
                        builder
                            .Append("        ")
                            .AppendLine("public static async Task<{Type}> Set{Name}(this IVisualElement<{type.FullName}> element, {Type} value)")
                            .AppendLine("        {")
                            .Append("            ")
                            .AppendLine("=> await element.SetProperty(nameof({type.FullName}.{Name}), value);")
                            .AppendLine("        }");
                    }
                }
                builder.AppendLine("    }");
                builder.AppendLine("}");

            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }
    }

    public record VisualElement(
        string Namespace, 
        VisualElementType Type,
        IReadOnlyList<DependencyProperty> DependencyProperties)
    { }

    public record VisualElementType(string Name, string FullName)
    { }

    public record DependencyProperty(bool CanRead, bool CanWrite)
    { }

    public class SyntaxReceiver : ISyntaxContextReceiver
    {
        public List<(string name, string template, string hash)> TemplateInfo = new();

        private List<VisualElement> Elements { get; } = new();
        public IReadOnlyList<VisualElement> GeneratedTypes => Elements;

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            // find all valid mustache attributes
            if (context.Node is AttributeSyntax attrib
                && attrib.ArgumentList?.Arguments.Count == 3
                && context.SemanticModel.GetTypeInfo(attrib).Type?.ToDisplayString() == "MustacheAttribute")
            {
                string name = context.SemanticModel.GetConstantValue(attrib.ArgumentList.Arguments[0].Expression).ToString();
                string template = context.SemanticModel.GetConstantValue(attrib.ArgumentList.Arguments[1].Expression).ToString();
                string hash = context.SemanticModel.GetConstantValue(attrib.ArgumentList.Arguments[2].Expression).ToString();

                TemplateInfo.Add((name, template, hash));
            }
        }
    }
}
