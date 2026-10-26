using FluentAssertions;
using System.Reflection;

namespace Afetto_.Net.Tests.Unit
{
    /// <summary>
    /// Garante a regra de dependência da Clean Architecture dentro do mesmo projeto:
    /// Domain não conhece ninguém; Application conhece só o Domain; Infrastructure não conhece a API.
    /// (Como as camadas são pastas/namespaces e não projetos, o compilador não impede violações — estes testes impedem.)
    /// </summary>
    public class ArchitectureTests
    {
        private const string Root = "Afetto_.Net.";
        private const string Domain = Root + "Domain";
        private const string Application = Root + "Application";
        private const string Infrastructure = Root + "Infrastructure";
        private static readonly string[] ApiNamespaces =
            { Root + "Controllers", Root + "Middleware", Root + "Hateoas", Root + "Extensions", Root + "Security" };

        private static readonly Assembly Assembly = typeof(Program).Assembly;

        private static IEnumerable<Type> TypesIn(string ns) =>
            Assembly.GetTypes().Where(t =>
                t.Namespace != null &&
                (t.Namespace == ns || t.Namespace.StartsWith(ns + ".")) &&
                !t.Name.StartsWith("<") &&                       // tipos gerados pelo compilador
                t.GetCustomAttribute<System.Runtime.CompilerServices.CompilerGeneratedAttribute>() == null);

        // Tipos referenciados por campos, propriedades, métodos, construtores, base e interfaces
        private static IEnumerable<Type> Dependencies(Type type)
        {
            const BindingFlags all = BindingFlags.Public | BindingFlags.NonPublic |
                                     BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            var found = new List<Type>();

            if (type.BaseType != null) found.Add(type.BaseType);
            found.AddRange(type.GetInterfaces());
            found.AddRange(type.GetFields(all).Select(f => f.FieldType));
            found.AddRange(type.GetProperties(all).Select(p => p.PropertyType));
            foreach (var m in type.GetMethods(all))
            {
                found.Add(m.ReturnType);
                found.AddRange(m.GetParameters().Select(p => p.ParameterType));
            }
            foreach (var c in type.GetConstructors(all))
                found.AddRange(c.GetParameters().Select(p => p.ParameterType));

            return found.SelectMany(Flatten);
        }

        // List<Foo> -> List, Foo ; Foo[] -> Foo
        private static IEnumerable<Type> Flatten(Type type)
        {
            if (type.HasElementType) return Flatten(type.GetElementType()!);
            if (!type.IsGenericType) return new[] { type };
            return new[] { type }.Concat(type.GetGenericArguments().SelectMany(Flatten));
        }

        private static bool IsIn(Type t, string ns) =>
            t.Namespace != null && (t.Namespace == ns || t.Namespace.StartsWith(ns + "."));

        private static void AssertNoDependency(string fromNs, params string[] forbiddenNs)
        {
            var violations = TypesIn(fromNs)
                .SelectMany(t => Dependencies(t)
                    .Where(d => forbiddenNs.Any(f => IsIn(d, f)))
                    .Select(d => $"{t.FullName} -> {d.FullName}"))
                .Distinct()
                .ToList();

            violations.Should().BeEmpty($"{fromNs} não pode depender de {string.Join(", ", forbiddenNs)}");
        }

        [Fact]
        public void Domain_NaoDependeDeOutrasCamadas()
        {
            TypesIn(Domain).Should().NotBeEmpty();
            AssertNoDependency(Domain, new[] { Application, Infrastructure }.Concat(ApiNamespaces).ToArray());
        }

        [Fact]
        public void Domain_NaoReferenciaFrameworksDePersistenciaOuWeb()
        {
            var proibidos = new[] { "MongoDB", "Microsoft.EntityFrameworkCore", "Microsoft.AspNetCore" };
            var violations = TypesIn(Domain)
                .SelectMany(t => Dependencies(t)
                    .Where(d => d.Namespace != null && proibidos.Any(p => d.Namespace.StartsWith(p)))
                    .Select(d => $"{t.FullName} -> {d.FullName}"))
                .ToList();

            violations.Should().BeEmpty();
        }

        [Fact]
        public void Application_NaoDependeDeInfrastructureNemDaApi()
        {
            TypesIn(Application).Should().NotBeEmpty();
            AssertNoDependency(Application, new[] { Infrastructure }.Concat(ApiNamespaces).ToArray());
        }

        [Fact]
        public void Infrastructure_NaoDependeDaApi()
        {
            TypesIn(Infrastructure).Should().NotBeEmpty();
            AssertNoDependency(Infrastructure, ApiNamespaces);
        }

        [Fact]
        public void Repositorios_ImplementamInterfacesDoDomain()
        {
            var implementacoes = TypesIn(Infrastructure + ".Repositories")
                .Concat(TypesIn(Infrastructure + ".Mongo"))
                .Where(t => t.Name.EndsWith("Repository") && t.IsClass);

            implementacoes.Should().NotBeEmpty();
            implementacoes.Should().OnlyContain(t => t.GetInterfaces().Any(i => IsIn(i, Domain + ".Interfaces")));
        }

        [Fact]
        public void Controllers_NaoUsamRepositoriosDiretamente()
        {
            var violations = TypesIn(Root + "Controllers")
                .SelectMany(t => Dependencies(t)
                    .Where(d => IsIn(d, Infrastructure) || IsIn(d, Domain + ".Interfaces"))
                    .Select(d => $"{t.FullName} -> {d.FullName}"))
                .ToList();

            // AuditoriaController consulta o repositório de auditoria (leitura simples, sem regra de negócio)
            violations.Where(v => !v.StartsWith(Root + "Controllers.AuditoriaController")).Should().BeEmpty();
        }
    }
}
