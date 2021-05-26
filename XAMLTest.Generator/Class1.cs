using Microsoft.CodeAnalysis;
using System;
using System.Net.Mime;

namespace XAMLTest.Generator
{
    [Generator]
    public class ElementGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
           
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            // No initialization required for this one
        }

    }
}
