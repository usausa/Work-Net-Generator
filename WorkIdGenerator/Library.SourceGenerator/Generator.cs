namespace Library.SourceGenerator;

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

[Generator]
public sealed class Generator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(AddAttribute);

        var viewProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => IsViewTargetSyntax(node),
                static (context, _) => GetViewModel(context))
            .Where(x => x is not null)
            .Collect();

        var sourceProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => IsSourceTargetSyntax(node),
                static (context, _) => GetSourceModel(context))
            .Where(x => x is not null)
            .Collect();

        context.RegisterImplementationSourceOutput(
            viewProvider.Combine(sourceProvider),
            static (context, provider) => Execute(context, provider.Left!, provider.Right!));
    }

    private static bool IsViewTargetSyntax(SyntaxNode node) =>
        node is ClassDeclarationSyntax { AttributeLists.Count: > 0 };

    private static bool IsSourceTargetSyntax(SyntaxNode node) =>
        node is MethodDeclarationSyntax { AttributeLists.Count: > 0 };

    private static ViewModel? GetViewModel(GeneratorSyntaxContext context)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;
        if (ModelExtensions.GetDeclaredSymbol(context.SemanticModel, classDeclarationSyntax) is not ITypeSymbol typeSymbol)
        {
            return null;
        }

        var attribute = typeSymbol.GetAttributes()
            .FirstOrDefault(x => x.AttributeClass!.ToDisplayString() == "Library.ViewAttribute");
        if (attribute is null)
        {
            return null;
        }

        if (attribute.ConstructorArguments.Length == 0)
        {
            return null;
        }

        var viewIdType = attribute.ConstructorArguments[0].Type;
        if (viewIdType is null)
        {
            return null;
        }

        return new ViewModel(
            typeSymbol.ToDisplayString(),
            viewIdType.ToDisplayString(),
            attribute.ConstructorArguments[0].ToCSharpString(),
            attribute.ConstructorArguments[0].Value);
    }

    private static SourceModel? GetSourceModel(GeneratorSyntaxContext context)
    {
        var methodDeclarationSyntax = (MethodDeclarationSyntax)context.Node;
        if (methodDeclarationSyntax.ParameterList.Parameters.Count != 1)
        {
            return null;
        }

        var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclarationSyntax);
        if ((methodSymbol is null) || !methodSymbol.IsPartialDefinition || !methodSymbol.IsStatic)
        {
            return null;
        }

        var attribute = methodSymbol.GetAttributes()
            .FirstOrDefault(x => x.AttributeClass!.ToDisplayString() == "Library.ViewRegistration");
        if (attribute is null)
        {
            return null;
        }

        if (methodSymbol.ReturnType.SpecialType != SpecialType.System_Void)
        {
            return null;
        }

        if (methodSymbol.Parameters.Length != 1)
        {
            return null;
        }

        var actionType = methodSymbol.Parameters[0].Type;
        if (actionType is not INamedTypeSymbol actionSymbol)
        {
            return null;
        }

        if ((actionSymbol.TypeArguments.Length != 2) ||
            (actionSymbol.ConstructedFrom.ToDisplayString() != "System.Action<T1, T2>") ||
            actionSymbol.TypeArguments[1].ToDisplayString() != "System.Type")
        {
            return null;
        }

        var containingType = methodSymbol.ContainingType;
        var ns = String.IsNullOrEmpty(containingType.ContainingNamespace.Name)
            ? string.Empty
            : containingType.ContainingNamespace.ToDisplayString();

        return new SourceModel(
            ns,
            containingType.Name,
            containingType.IsValueType,
            methodSymbol.DeclaredAccessibility,
            methodSymbol.Name,
            methodSymbol.Parameters[0].Type.ToDisplayString(),
            methodSymbol.Parameters[0].Name,
            actionSymbol.TypeArguments[0].ToDisplayString());
    }

    private static void Execute(SourceProductionContext context, ImmutableArray<ViewModel> viewModels, ImmutableArray<SourceModel> sourceModels)
    {
        var viewModelMap = viewModels
            .GroupBy(x => x.ViewIdClassFullName)
            .ToDictionary(x => x.Key, x => x.ToList());

        var buffer = new StringBuilder();

        foreach (var sourceModel in sourceModels)
        {
            buffer.Clear();

            buffer.AppendLine("// <auto-generated />");
            buffer.AppendLine("#nullable enable");

            // namespace
            if (!String.IsNullOrEmpty(sourceModel.Namespace))
            {
                buffer.Append("namespace ").Append(sourceModel.Namespace).AppendLine();
            }

            buffer.AppendLine("{");

            // class
            buffer.Append("    partial ").Append(sourceModel.IsValueType ? "struct " : "class ").Append(sourceModel.ClassName).AppendLine();
            buffer.AppendLine("    {");

            // method
            buffer.Append("        ");
            buffer.Append(ToAccessibilityText(sourceModel.MethodAccessibility));
            buffer.Append(" static partial void ");
            buffer.Append(sourceModel.MethodName);
            buffer.Append("(");
            buffer.Append(sourceModel.ArgumentType);
            buffer.Append(' ');
            buffer.Append(sourceModel.ArgumentName);
            buffer.Append(")");
            buffer.AppendLine();

            buffer.AppendLine("        {");

            if (viewModelMap.TryGetValue(sourceModel.ViewIdClassFullName, out var views))
            {
                foreach (var viewModel in views.OrderBy(x => x.Value))
                {
                    buffer.Append("            ");
                    buffer.Append(sourceModel.ArgumentName);
                    buffer.Append('(');
                    buffer.Append(viewModel.ViewIdFullName);
                    buffer.Append(", ");
                    buffer.Append("typeof(");
                    buffer.Append(viewModel.ClassFullName);
                    buffer.Append("));");
                    buffer.AppendLine();
                }
            }

            buffer.AppendLine("        }");

            buffer.AppendLine("    }");
            buffer.AppendLine("}");

            var source = buffer.ToString();
            var filename = MakeRegistryFilename(buffer, sourceModel.Namespace, sourceModel.ClassName);
            context.AddSource(filename, SourceText.From(source, Encoding.UTF8));
        }
    }

    private static string MakeRegistryFilename(StringBuilder buffer, string ns, string className)
    {
        buffer.Clear();

        if (!String.IsNullOrEmpty(ns))
        {
            buffer.Append(ns.Replace('.', '_'));
            buffer.Append('_');
        }

        buffer.Append(className);
        buffer.Append(".g.cs");

        return buffer.ToString();
    }

    private static string ToAccessibilityText(Accessibility accessibility) => accessibility switch
    {
        Accessibility.Public => "public",
        Accessibility.Protected => "protected",
        Accessibility.Private => "private",
        Accessibility.Internal => "internal",
        Accessibility.ProtectedOrInternal => "protected internal",
        Accessibility.ProtectedAndInternal => "private protected",
        _ => throw new NotSupportedException()
    };

    private const string AttributeSource = @"// <auto-generated />
using System;

namespace Library
{
    [System.Diagnostics.Conditional(""COMPILE_TIME_ONLY"")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ViewRegistration : Attribute
    {
    }
}
";

    private static void AddAttribute(IncrementalGeneratorPostInitializationContext context)
    {
        context.AddSource("ViewRegistration", SourceText.From(AttributeSource, Encoding.UTF8));
    }

    internal sealed record ViewModel(
        string ClassFullName,
        string ViewIdClassFullName,
        string ViewIdFullName,
        object? Value);

    internal sealed record SourceModel(
        string Namespace,
        string ClassName,
        bool IsValueType,
        Accessibility MethodAccessibility,
        string MethodName,
        string ArgumentType,
        string ArgumentName,
        string ViewIdClassFullName);
}
