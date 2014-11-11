using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            var descriptors = typeof (DesignerBoundedContextModule)
                .Assembly
                .GetTypes()
                .Where(HasAttribute<MapsToAggregateRootMethodAttribute>)
                .Select(type => new
                {
                    Command = type,
                    Attribute = type.CustomAttributes.Single(attribute => attribute.AttributeType == typeof (MapsToAggregateRootMethodAttribute)),
                })
                .Select(x => new
                {
                    x.Command,
                    Aggregate = (Type) x.Attribute.ConstructorArguments.First().Value,
                    Method = (string)x.Attribute.ConstructorArguments.Last().Value,
                })
                .Select(x => new
                {
                    Aggregate = x.Aggregate.Name,
                    x.Method,
                    Command = x.Command.Name,
                    IdProperty = GetAggregateIdProperty(x.Command),
                    Parameters = GetParams(x.Aggregate, x.Method, x.Command)
                })
                .OrderBy(x => x.Command)
                .GroupBy(x => x.Aggregate);

            foreach (var aggregate in descriptors)
            {
                Console.WriteLine();
                Console.WriteLine(@"            CommandRegistry.For<Aggregates.{0}>()", aggregate.Key);

                foreach (var descriptor in aggregate)
                {
                    Console.WriteLine(@"                .Add<{0}>(", descriptor.Command);
                    Console.WriteLine(@"                    command => command.{0},", descriptor.IdProperty);
                    Console.WriteLine(@"                    (command, aggregate) => aggregate.{0}({1}))", descriptor.Method, string.Join(", ", descriptor.Parameters.Select(p => "command." + p)));
                }
                Console.WriteLine(@"            ;");
            }
        }

        private static IEnumerable<string> GetParams(Type aggregate, string methodName, Type command)
        {
            MethodInfo method = aggregate.GetMethod(methodName);
            
            IEnumerable<string> properties = command.GetProperties().Select(property => property.Name).ToList();
            IEnumerable<string> parameters = method.GetParameters().Select(parameter => parameter.Name).ToList();

            return parameters.Select(parameter => properties.Single(property => string.Equals(parameter, property, StringComparison.InvariantCultureIgnoreCase)));
        }

        private static string GetAggregateIdProperty(Type command)
        {
            return command.GetProperties().Single(HasAttribute<AggregateRootIdAttribute>).Name;
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
