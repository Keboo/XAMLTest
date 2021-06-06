﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace XAMLTest.Generator
{
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

                StringBuilder builder = new();
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

                        if (property.TypeFullName == "System.Windows.Media.SolidColorBrush" ||
                            property.TypeFullName == "System.Windows.Media.Brush")
                        {
                            builder
                            .Append("        ");
                            if (type.Type.IsFinal)
                            {
                                builder.AppendLine($"public static async System.Threading.Tasks.Task<System.Windows.Media.Color> Get{property.Name}Color(this IVisualElement<{type.Type.FullName}> element)");
                            }
                            else
                            {
                                builder.AppendLine($"public static async System.Threading.Tasks.Task<System.Windows.Media.Color> Get{property.Name}Color<T>(this IVisualElement<T> element) where T : {type.Type.FullName}");
                            }
                            builder
                            .Append("            ")
                            .AppendLine($"=> await element.GetProperty<System.Windows.Media.Color>(nameof({type.Type.FullName}.{property.Name}));");
                        }
                    }
                    if (property.CanWrite)
                    {
                        builder
                            .Append("        ");

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

    public record VisualElementType(string Name, string FullName, bool IsFinal)
    { }

    public record Property(string Name, string TypeFullName, bool CanRead, bool CanWrite)
    { }

    public class SyntaxReceiver : ISyntaxContextReceiver
    {
        private static HashSet<string> IgnoredTypes { get; } = new()
        {
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
            "System.Windows.DependencyObject", //This should be IVisualElement
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
            "System.Windows.Controls.ItemContainerTemplateSelector"
        };
        private List<VisualElement> Elements { get; } = new();
        public IReadOnlyList<VisualElement> GeneratedTypes => Elements;

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            // find all valid mustache attributes
            if (context.Node is AttributeSyntax attrib
                && attrib.ArgumentList?.Arguments.Count >= 1
                && context.SemanticModel.GetTypeInfo(attrib).Type?.Name == "GenerateHelpersAttribute")
            {
                TypeOfExpressionSyntax typeArgument = (TypeOfExpressionSyntax)attrib.ArgumentList.Arguments[0].Expression;
                TypeInfo info = context.SemanticModel.GetTypeInfo(typeArgument.Type);
                if (info.Type is null) return;

                string? targetNamespace = null;
                foreach (AttributeArgumentSyntax argumentExpression in attrib.ArgumentList.Arguments.Skip(1))
                {
                    string? target = argumentExpression.NameEquals?.Name.Identifier.Value?.ToString();

                    switch (target)
                    {
                        case "Namespace":
                            switch (argumentExpression.Expression)
                            {
                                case LiteralExpressionSyntax les:
                                    targetNamespace = les.Token.Value?.ToString();
                                    break;
                                case MemberAccessExpressionSyntax maes:
                                    targetNamespace = context.SemanticModel.GetConstantValue(maes.Name).Value?.ToString();
                                    break;
                            }
                            break;
                    }
                }

                for (ITypeSymbol? type = info.Type;
                    type is not null;
                    type = type.BaseType)
                {
                    List<Property> properties = new();

                    if (Elements.Any(x => x.Type.FullName == $"{type}")) continue;

                    foreach (ISymbol member in type.GetMembers())
                    {
                        if (member is IPropertySymbol property &&
                            property.CanBeReferencedByName &&
                            !property.IsStatic &&
                            property.DeclaredAccessibility == Accessibility.Public &&
                            !property.GetAttributes().Any(x => x.AttributeClass.Name == "ObsoleteAttribute") &&
                            !IgnoredTypes.Contains($"{property.Type}"))
                        {
                            if (IsFrameworkElement(property.Type))
                            {
                                properties.Add(
                                    new Property(
                                        property.Name,
                                        $"XamlTest.IVisualElement<{property.Type}>",
                                        property.GetMethod is not null,
                                        property.SetMethod is not null));
                            }
                            else
                            {
                                properties.Add(
                                    new Property(
                                        property.Name,
                                        $"{property.Type}",
                                        property.GetMethod is not null,
                                        property.SetMethod is not null));
                            }
                        }
                    }
                    if (properties.Any())
                    {
                        var visualElementType = new VisualElementType(type.Name, $"{type}", type.IsSealed || type.IsValueType);
                        Elements.Add(new VisualElement(targetNamespace ?? "XamlTest", visualElementType, properties));
                    }
                }
            }
        }

        private static bool IsFrameworkElement(ITypeSymbol typeSymbol)
        {
            for (ITypeSymbol? type = typeSymbol;
                type != null;
                type = type.BaseType)
            {
                if ($"{type}" == "System.Windows.FrameworkElement")
                {
                    return true;
                }
            }
            return false;
        }
    }
}