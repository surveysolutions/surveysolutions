using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer;

namespace WB.Tools.CommandMapper
{
    class Program
    {
        static void Main(string[] args)
        {
            Type[] types = typeof (DesignerBoundedContextModule)
                .Assembly
                .GetTypes();

            IEnumerable<Tuple<Type, CustomAttributeData, bool>> commandsAndAttributes =
                ConcatMany(
                    types
                        .Where(HasAttribute<MapsToAggregateRootConstructorAttribute>)
                        .Select(type => Tuple.Create(type, GetAttribute<MapsToAggregateRootConstructorAttribute>(type), true)),
                    types
                        .Where(HasAttribute<MapsToAggregateRootMethodAttribute>)
                        .Select(type => Tuple.Create(type, GetAttribute<MapsToAggregateRootMethodAttribute>(type), false)),
                    types
                        .Where(HasAttribute<MapsToAggregateRootMethodOrConstructorAttribute>)
                        .Select(type => Tuple.Create(type, GetAttribute<MapsToAggregateRootMethodOrConstructorAttribute>(type), true)));

            var descriptors = commandsAndAttributes
                .Select(x => new
                {
                    Command = x.Item1,
                    Aggregate = GetAggregateType(x.Item2),
                    Method = GetMethodName(x.Item1, x.Item2),
                    IsConstructor = x.Item3,
                })
                .Select(x => new
                {
                    Aggregate = x.Aggregate.Name,
                    x.Method,
                    x.IsConstructor,
                    Command = x.Command.Name,
                    IdProperty = GetAggregateIdProperty(x.Command),
                    Parameters = GetParams(x.Aggregate, x.Method, x.Command)
                })
                .OrderByDescending(x => x.IsConstructor)
                .ThenBy(x => x.Method)
                .GroupBy(x => x.Aggregate)
                .OrderBy(x => x.Key);

            foreach (var aggregate in descriptors)
            {
                Console.WriteLine();
                Console.WriteLine(@"            CommandRegistry");
                Console.WriteLine(@"                .Setup<Aggregates.{0}>()", aggregate.Key);

                foreach (var descriptor in aggregate)
                {
                    Console.Write(@"                .{1}<{0}>(", descriptor.Command, descriptor.IsConstructor ? "InitializedWith" : "Handles");
                    Console.Write(@"command => command.{0}, ", descriptor.IdProperty);
                    Console.WriteLine(@"(command, aggregate) => aggregate.{0}({1}))", descriptor.Method, string.Join(", ", descriptor.Parameters.Select(p => "command." + p)));
                }
                Console.WriteLine(@"            ;");
            }
        }

        private static string GetMethodName(Type command, CustomAttributeData attribute)
        {
            return attribute.ConstructorArguments.Count == 2 ? (string) attribute.ConstructorArguments.Last().Value : command.Name.Replace("Command", "");
        }

        private static Type GetAggregateType(CustomAttributeData attribute)
        {
            return (Type) attribute.ConstructorArguments.First().Value;
        }

        private static IEnumerable<T> ConcatMany<T>(IEnumerable<T> first, IEnumerable<T> second, IEnumerable<T> third)
        {
            return first.Concat(second).Concat(third);
        }

        private static CustomAttributeData GetAttribute<TAttribute>(Type type)
        {
            return type.CustomAttributes.Single(attribute => attribute.AttributeType == typeof (TAttribute));
        }

        private static IEnumerable<string> GetParams(Type aggregate, string methodName, Type command)
        {
            IEnumerable<string> properties = command
                .GetProperties()
                .Select(property => property.Name)
                .Where(property => property != "CommandIdentifier" && property != "KnownVersion")
                .ToList();

            MethodInfo method = aggregate.GetMethod(methodName);

            if (method != null)
            {
                IEnumerable<string> parameters = method
                    .GetParameters()
                    .Select(parameter => parameter.Name)
                    .ToList();

                return parameters.Select(parameter => properties.Single(property => string.Equals(parameter, property, StringComparison.InvariantCultureIgnoreCase)));
            }
            else
            {
                return properties;
            }
        }

        private static string GetAggregateIdProperty(Type command)
        {
            var idProperty = command.GetProperties().SingleOrDefault(HasAttribute<AggregateRootIdAttribute>);
            
            return idProperty != null ? idProperty.Name : "?";
        }

        private static bool HasAttribute<TAttribute>(PropertyInfo property)
        {
            return property.CustomAttributes.Any(attribute => attribute.AttributeType == typeof(TAttribute));
        }

        private static bool HasAttribute<TAttribute>(Type type)
        {
            return type.CustomAttributes.Any(attribute => attribute.AttributeType == typeof(TAttribute));
        }
    }
}
