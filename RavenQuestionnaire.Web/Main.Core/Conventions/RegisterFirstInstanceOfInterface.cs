using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ninject.Extensions.Conventions.BindingGenerators;
using Ninject.Syntax;

namespace Main.Core.Conventions
{
    /// <summary>
    /// The register first instance of interface.
    /// </summary>
    public class RegisterFirstInstanceOfInterface : IBindingGenerator
    {
        public RegisterFirstInstanceOfInterface(IEnumerable<Assembly> assemblyes)
        {
            this._assemblyes = assemblyes;
        }

        private readonly IEnumerable<Assembly> _assemblyes;

        public IEnumerable<IBindingWhenInNamedWithOrOnSyntax<object>> CreateBindings(
            Type type, IBindingRoot bindingRoot)
        {
            IEnumerable<IBindingWhenInNamedWithOrOnSyntax<object>> y =
                Enumerable.Empty<IBindingWhenInNamedWithOrOnSyntax<object>>();

            if (!type.IsInterface)
            {
                return y;
            }


            if (type.IsGenericType)
            {
                return y;
            }

            Type matchedType = _assemblyes
                .SelectMany(a=>a.GetTypes().Where(t => t.IsVisible))
                .FirstOrDefault(
                    x => !x.IsAbstract && x.GetInterface(type.FullName) != null);
            if (matchedType == null)
            {
                return y;
            }

            return new[] { bindingRoot.Bind(new[] { type }).To(matchedType) };
        }
    }
}