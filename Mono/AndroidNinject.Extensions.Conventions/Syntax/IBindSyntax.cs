//-------------------------------------------------------------------------------
// <copyright file="IBindSyntax.cs" company="Ninject Project Contributors">
//   Copyright (c) 2009-2011 Ninject Project Contributors
//   Authors: Remo Gloor (remo.gloor@gmail.com)
//           
//   Dual-licensed under the Apache License, Version 2.0, and the Microsoft Public License (Ms-PL).
//   you may not use this file except in compliance with one of the Licenses.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//   or
//       http://www.microsoft.com/opensource/licenses.mspx
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>
//-------------------------------------------------------------------------------

namespace Ninject.Extensions.Conventions.Syntax
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    using Ninject.Extensions.Conventions.BindingGenerators;
#if !SILVERLIGHT_20 && !WINDOWS_PHONE && !NETCF_35 && !MONO
    using Ninject.Extensions.Factory;
#endif
    using Ninject.Syntax;

    /// <summary>
    /// Delegate to select the types for which a binding is created to the given type.
    /// </summary>
    /// <param name="type">The type for which the bindings are created.</param>
    /// <param name="baseTypes">The base types of the given type.</param>
    /// <returns>The types for which a binding is created to the given type.</returns>
    public delegate IEnumerable<Type> ServiceSelector(Type type, IEnumerable<Type> baseTypes);

    /// <summary>
    /// The syntax to define how the types are bound.
    /// </summary>
    public interface IBindSyntax : IFluentSyntax
    {
        /// <summary>
        /// Bind using a custom binding generator.
        /// </summary>
        /// <typeparam name="T">The type of the binding generator</typeparam>
        /// <returns>The fluent syntax</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Makes the API simpler.")]
        IConfigureSyntax BindWith<T>() where T : IBindingGenerator, new();

        /// <summary>
        /// Bind using a custom binding generator.
        /// </summary>
        /// <param name="generator">The generator used to create the bindings.</param>
        /// <returns>The fluent syntax</returns>
        IConfigureSyntax BindWith(IBindingGenerator generator);

        /// <summary>
        /// Binds all interfaces of the given types to the type.
        /// </summary>
        /// <returns>The fluent syntax</returns>
        IConfigureSyntax BindAllInterfaces();

        /// <summary>
        /// Binds the base type of the given types to the type.
        /// </summary>
        /// <returns>The fluent syntax</returns>
        IConfigureSyntax BindBase();

        /// <summary>
        /// Binds the default interface of the given types to the type.
        /// e.g. Foo : IFoo 
        /// </summary>
        /// <returns>The fluent syntax</returns>
        IConfigureSyntax BindDefaultInterface();

        /// <summary>
        ///  Binds the default interface of the given types to the type.
        /// e.g. MyFoo matches IFoo, and SuperCrazyFoo matches ICrazyFoo and IFoo
        /// </summary>
        /// <returns>The fluent syntax</returns>
        IConfigureSyntax BindDefaultInterfaces();

        /// <summary>
        /// Expects that the given type has a single interface.
        /// In this case the interface is bound to the type.
        /// </summary>
        /// <returns>The fluent syntax</returns>
        IConfigureSyntax BindSingleInterface();

        /// <summary>
        /// Binds the type to itself.
        /// </summary>
        /// <returns>The fluent syntax</returns>
        IConfigureSyntax BindToSelf();

        /// <summary>
        /// Binds the selected interfaces to the type.
        /// </summary>
        /// <param name="selector">The selector of the interfaces.</param>
        /// <returns>The fluent syntax</returns>
        IConfigureSyntax BindSelection(ServiceSelector selector);

        /// <summary>
        /// Bind the type to its interfaces matching the given regular expression.
        /// </summary>
        /// <param name="pattern">The regular expression.</param>
        /// <returns>The fluent syntax</returns>
        IConfigureSyntax BindUsingRegex(string pattern);

        /// <summary>
        /// Bind the type to its interfaces matching the given regular expression.
        /// </summary>
        /// <param name="pattern">The regular expression.</param>
        /// <param name="options">The regex options.</param>
        /// <returns>The fluent syntax</returns>
        IConfigureSyntax BindUsingRegex(string pattern, RegexOptions options);

#if !SILVERLIGHT_20 && !WINDOWS_PHONE && !NETCF_35 && !MONO
        /// <summary>
        /// Binds interfaces to factory implementations using the factory extension.
        /// </summary>
        /// <returns>The fluent syntax</returns>
        IConfigureSyntax BindToFactory();

        /// <summary>
        /// Binds interfaces to factory implementations using the factory extension.
        /// </summary>
        /// <param name="instanceProvider">The instance provider.</param>
        /// <returns>The fluent syntax</returns>
        IConfigureSyntax BindToFactory(Func<IInstanceProvider> instanceProvider);
#endif
    }
}