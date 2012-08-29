﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RegisterFirstInstanceOfInterface.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The register first instance of interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Conventions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Ninject.Extensions.Conventions.BindingGenerators;
    using Ninject.Syntax;

    /// <summary>
    /// The register first instance of interface.
    /// </summary>
    public class RegisterFirstInstanceOfInterface : IBindingGenerator
    {
        /* public void Process(Type type, Func<IContext, object> scopeCallback, IKernel kernel)
        {
            if (!type.IsInterface)
            {
                return;
            }
            if (type.IsGenericTypeDefinition)
            {
                return;
            }

            Assembly containingAssembly = type.Assembly;
            var matchedType = containingAssembly.GetTypes().FirstOrDefault(x => x.Namespace == type.Namespace
                                                                                && x.GetInterface(type.FullName) != null);
            if (matchedType == null)
            {
                return;
            }
            if(kernel.TryGet(type)!=null)
                return;
            kernel.Bind(type).To(matchedType);
          //  registry.For(type).Use(matchedType);
        }*/
        #region Public Methods and Operators

        /// <summary>
        /// The create bindings.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="bindingRoot">
        /// The binding root.
        /// </param>
        /// <returns>
        /// The System.Collections.Generic.IEnumerable`1[T -&gt; Ninject.Syntax.IBindingWhenInNamedWithOrOnSyntax`1[T -&gt; System.Object]].
        /// </returns>
        public IEnumerable<IBindingWhenInNamedWithOrOnSyntax<object>> CreateBindings(
            Type type, IBindingRoot bindingRoot)
        {
            IEnumerable<IBindingWhenInNamedWithOrOnSyntax<object>> y =
                Enumerable.Empty<IBindingWhenInNamedWithOrOnSyntax<object>>();

            if (!type.IsInterface)
            {
                return y;
            }

            Assembly containingAssembly = type.Assembly;

            if (type.IsGenericType)
            {
                return y;
            }

            Type matchedType =
                containingAssembly.GetTypes().FirstOrDefault(
                    x => x.Namespace == type.Namespace && !x.IsAbstract && x.GetInterface(type.FullName) != null);
            if (matchedType == null)
            {
                return y;
            }

            return new[]
                {
                   bindingRoot.Bind(new[] { type }).To(matchedType) 
                };
        }

        #endregion
    }
}