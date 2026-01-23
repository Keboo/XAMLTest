using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TypeInfo = Microsoft.CodeAnalysis.TypeInfo;

namespace XAMLTest.UnitTestGenerator;

public partial class UnitTestGenerator
{
    public class SyntaxReceiver : ISyntaxContextReceiver
    {
        public List<TypeInfo> GeneratedTypes { get; } = new();

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is AttributeSyntax attrib
                && attrib.ArgumentList?.Arguments.Count >= 1
                && context.SemanticModel.GetTypeInfo(attrib).Type?.Name == "GenerateTestsAttribute")
            {
                TypeOfExpressionSyntax typeArgument = (TypeOfExpressionSyntax)attrib.ArgumentList.Arguments[0].Expression;
                TypeInfo info = context.SemanticModel.GetTypeInfo(typeArgument.Type);
                if (info.Type is null) return;

                GeneratedTypes.Add(info);
            }
        }
    }
}
