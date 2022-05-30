using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using TypeInfo = Microsoft.CodeAnalysis.TypeInfo;

namespace XAMLTest.Generator;

public class SyntaxReceiver : ISyntaxContextReceiver
{
    private static Dictionary<string, string> TypeRemap { get; } = new()
    {
        { "System.Windows.Controls.ColumnDefinitionCollection", "System.Collections.Generic.IList<System.Windows.Controls.ColumnDefinition>" },
        { "System.Windows.Controls.RowDefinitionCollection", "System.Collections.Generic.IList<System.Windows.Controls.RowDefinition>" }
    };

    private static HashSet<string> IgnoredTypes { get; } = new()
    {
        //WPF Types
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
        "System.Windows.WindowCollection",

        //WinUI3
        "Microsoft.UI.Xaml.Input.InputScope"
    };
    private List<VisualElement> Elements { get; } = new();
    public IReadOnlyList<VisualElement> GeneratedTypes => Elements;

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
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
                HashSet<Property> properties = new();

                if (Elements.Any(x => x.Type.FullName == $"{type}")) continue;

                IPropertySymbol? GetProperty(ISymbol symbol)
                    => symbol as IPropertySymbol ??
                      (symbol as IMethodSymbol)?.AssociatedSymbol as IPropertySymbol;

                foreach (ISymbol member in type.GetMembers())
                {
                    if (GetProperty(member) is { } property &&
                        property.CanBeReferencedByName &&
                        !property.IsStatic &&
                        !property.IsOverride &&
                        property.DeclaredAccessibility == Accessibility.Public &&
                        !property.GetAttributes().Any(x => x.AttributeClass.Name == "ObsoleteAttribute") &&
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
                    var visualElementType = new VisualElementType(type.Name, $"{type}", type.IsSealed || type.IsValueType);
                    Elements.Add(new VisualElement(targetNamespace ?? "XamlTest", visualElementType, properties.OrderBy(x => x.Name).ToList()));
                }
            }
        }

        static bool ShouldUseVisualElement(ITypeSymbol typeSymbol)
        {
            for (ITypeSymbol? type = typeSymbol;
                 type != null;
                 type = type.BaseType)
            {
                switch($"{type}")
                {
                    //WPF
                    case "System.Windows.Media.Brush": return false;
                    case "System.Windows.DependencyObject": return true;

                    //WinUI3
                    case "Microsoft.UI.Xaml.Media.Brush": return false;
                    case "Microsoft.UI.Xaml.DependencyObject": return true;
                }
            }
            return false;
        }
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
