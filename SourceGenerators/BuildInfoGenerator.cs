using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System.Security.Cryptography;

namespace SourceGenerators;

[Generator]
public class BuildInfoGenerator : IIncrementalGenerator
{
    public class GeneratorConfig
    {
        public string? Namespace { get; set; } = "Generated";
        public string? Prefix { get; set; } = "Auto";
        public bool EnableDebugOutput { get; set; } = false;
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Get MSBuild properties
        var configProvider = context.AnalyzerConfigOptionsProvider.Select(static (provider, _) => GetConfiguration(provider));
        // Get all types from compilation with stable ordering and comprehensive collection
        var compilationConfigProvider = configProvider.Combine(context.CompilationProvider);
        var typeCollector = compilationConfigProvider.Select((data, ct) =>
        {
            var (config, compilation) = data;
#if DEBUG
            // Add debugging trigger based on configuration
            if ( config.EnableDebugOutput && !System.Diagnostics.Debugger.IsAttached )
            {
                System.Diagnostics.Debugger.Launch();
            }
#endif
            return compilation.GetSymbolsWithName(_ => true, SymbolFilter.Type, ct)
                .Where(t => IsTargetType(config, t))
                .OfType<INamedTypeSymbol>()
                .Select(CollectTypeInfo)
                .OrderBy(t => t.FullName) // Stable ordering
                .ToArray();
        });

        var combined = configProvider.Combine(typeCollector);

        context.RegisterSourceOutput(combined,
            static (spc, data) => GenerateCode(spc, data.Left, data.Right));
    }

    private static bool IsTargetType(GeneratorConfig config, ISymbol t)
    {
        return string.IsNullOrWhiteSpace(config.Namespace) || Equals(t.ContainingNamespace?.ToString(), config.Namespace);
    }

    private static TypeInfo CollectTypeInfo(INamedTypeSymbol type)
    {
        return new TypeInfo {
            FullName = GetFullyQualifiedName(type),
            TypeKind = type.TypeKind.ToString(),
            Accessibility = type.DeclaredAccessibility.ToString(),
            IsAbstract = type.IsAbstract,
            IsSealed = type.IsSealed,
            IsStatic = type.IsStatic,
            IsGeneric = type.IsGenericType,
            GenericParameters = type.TypeParameters.Select(tp => tp.Name).OrderBy(x => x).ToArray(),
            BaseType = type.BaseType?.ToDisplayString() ?? "",
            Interfaces = type.Interfaces.Select(i => i.ToDisplayString()).OrderBy(x => x).ToArray(),
            Members = CollectTypeMembers(type).OrderBy(m => m.Signature).ToArray()
        };
    }

    private static string GetFullyQualifiedName(INamedTypeSymbol type)
    {
        var sb = new StringBuilder();

        // Add namespace
        if ( type.ContainingNamespace != null && !type.ContainingNamespace.IsGlobalNamespace )
        {
            sb.Append(type.ContainingNamespace.ToDisplayString());
            sb.Append('.');
        }

        // Add containing types (for nested types)
        var containingTypes = new List<string>();
        var current = type.ContainingType;
        while ( current != null )
        {
            containingTypes.Insert(0, current.Name);
            current = current.ContainingType;
        }

        foreach ( var containingType in containingTypes )
        {
            sb.Append(containingType);
            sb.Append('.');
        }

        // Add type name
        sb.Append(type.Name);

        return sb.ToString();
    }

    private static IEnumerable<MemberInfo> CollectTypeMembers(INamedTypeSymbol type)
    {
        return type.GetMembers()
            .Where(FilterTrackedMembers)
            .Select(m => new MemberInfo {
                Name = m.Name,
                Kind = m.Kind.ToString(),
                Accessibility = m.DeclaredAccessibility.ToString(),
                IsStatic = m.IsStatic,
                IsVirtual = m.IsVirtual,
                IsOverride = m.IsOverride,
                IsAbstract = m.IsAbstract,
                Signature = GetMemberSignature(m)
            });
    }

    private static bool FilterTrackedMembers(ISymbol m)
    {
        return !m.IsImplicitlyDeclared
               && !m.IsStatic
               && m.DeclaredAccessibility != Accessibility.Private && m.DeclaredAccessibility != Accessibility.ProtectedAndInternal && m.DeclaredAccessibility != Accessibility.Internal && m.DeclaredAccessibility != Accessibility.Protected
               && (m.Kind == SymbolKind.Property || m.Kind == SymbolKind.Field)
            ;
    }

    private static string GetMemberSignature(ISymbol member)
    {
        return member switch {
            IMethodSymbol method => $"{method.ReturnType.ToDisplayString()} {method.Name}({string.Join(", ", method.Parameters.Select(p => $"{p.Type.ToDisplayString()} {p.Name}"))})",
            IPropertySymbol property => $"{property.Type.ToDisplayString()} {property.Name} {{ {(property.GetMethod != null ? "get; " : "")}{(property.SetMethod != null ? "set; " : "")}}}",
            IFieldSymbol field => $"{field.Type.ToDisplayString()} {field.Name}",
            IEventSymbol eventSymbol => $"event {eventSymbol.Type.ToDisplayString()} {eventSymbol.Name}",
            _ => member.ToDisplayString()
        };
    }

    private static GeneratorConfig GetConfiguration(AnalyzerConfigOptionsProvider provider)
    {
        var config = new GeneratorConfig();

        // Get global analyzer config options (MSBuild properties)
        if ( provider.GlobalOptions.TryGetValue("build_property.HashGeneratorNamespace", out var namespaceValue) )
        {
            config.Namespace = namespaceValue;
        }

        if ( provider.GlobalOptions.TryGetValue("build_property.MyGeneratorPrefix", out var prefixValue) )
        {
            config.Prefix = prefixValue;
        }

        if ( provider.GlobalOptions.TryGetValue("build_property.EnableDebugOutput", out var debugValue) )
        {
            config.EnableDebugOutput = bool.TryParse(debugValue, out var result) && result;
        }

        return config;
    }

    private static void GenerateCode(SourceProductionContext context, GeneratorConfig config, TypeInfo[] types)
    {
        var buildInfo = $@"
        // Auto-generated at build time
        using System;

        namespace {config.Namespace}
        {{
            public class {config.Prefix}TypesStableHash
            {{
                public string TypesStableHash = ""{ComputeBuildHash(types)}"";
            }}
        }}";

        context.AddSource($"{config.Prefix}TypesStableHash.g.cs", SourceText.From(buildInfo, Encoding.UTF8));
    }

    private static string ComputeBuildHash(TypeInfo[] types)
    {
        using var sha256 = SHA256.Create(); // SHA256 is more robust than MD5

        var sb = new StringBuilder();

        foreach ( var type in types.OrderBy(t => t.FullName) )
        {
            sb.AppendLine($"Type: {type.FullName}");
            sb.AppendLine($"Kind: {type.TypeKind}");
            foreach ( var member in type.Members )
            {
                sb.AppendLine($"  Member: {member.Name}");
                sb.AppendLine($"    Kind: {member.Kind}");
                sb.AppendLine($"    Signature: {member.Signature}");
            }
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        sb.Clear();
        var hash = sha256.ComputeHash(bytes);

        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    private struct TypeInfo
    {
        public string FullName { get; set; } = "";
        public string TypeKind { get; set; } = "";
        public string Accessibility { get; set; } = "";
        public bool IsAbstract { get; set; }
        public bool IsSealed { get; set; }
        public bool IsStatic { get; set; }
        public bool IsGeneric { get; set; }
        public string[] GenericParameters { get; set; } = Array.Empty<string>();
        public string BaseType { get; set; } = "";
        public string[] Interfaces { get; set; } = Array.Empty<string>();
        public MemberInfo[] Members { get; set; } = Array.Empty<MemberInfo>();

        public TypeInfo()
        { }
    }

    private struct MemberInfo
    {
        public string Name { get; set; } = "";
        public string Kind { get; set; } = "";
        public string Accessibility { get; set; } = "";
        public bool IsStatic { get; set; }
        public bool IsVirtual { get; set; }
        public bool IsOverride { get; set; }
        public bool IsAbstract { get; set; }
        public string Signature { get; set; } = "";

        public MemberInfo()
        { }
    }
}