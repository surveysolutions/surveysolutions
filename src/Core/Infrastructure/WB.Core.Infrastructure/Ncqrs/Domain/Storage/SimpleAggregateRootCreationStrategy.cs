using System;
using System.Linq;
using System.Reflection;

namespace Ncqrs.Domain.Storage
{
    public class SimpleAggregateRootCreationStrategy 
        : AggregateRootCreationStrategy
    {

        protected override AggregateRoot CreateAggregateRootFromType(Type aggregateRootType)
        {
            // Get the constructor that we want to invoke.
            var ctor = aggregateRootType
                .GetTypeInfo()
                .DeclaredConstructors
                .Where(x => !x.GetParameters().Any())
                .FirstOrDefault(x => x.IsPublic);

            // If there was no ctor found, throw exception.
            if (ctor == null)
            {
                var message = String.Format("No constructor found on aggregate root type {0} that accepts " +
                                            "no parameters.", aggregateRootType.AssemblyQualifiedName);
                throw new AggregateRootCreationException(message);
            }

            // There was a ctor found, so invoke it and return the instance.
            var aggregateRoot = (AggregateRoot)(ctor.Invoke(new object[0]));

            return aggregateRoot;
        }

    }
}
