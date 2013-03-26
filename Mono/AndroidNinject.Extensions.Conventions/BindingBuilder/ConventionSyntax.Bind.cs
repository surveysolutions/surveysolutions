//-------------------------------------------------------------------------------
// <copyright file="ConventionSyntax.Bind.cs" company="Ninject Project Contributors">
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

namespace Ninject.Extensions.Conventions.BindingBuilder
{
    using System;
    using System.Text.RegularExpressions;

    using Ninject.Extensions.Conventions.BindingGenerators;
    using Ninject.Extensions.Conventions.Syntax;
#if !SILVERLIGHT_20 && !WINDOWS_PHONE && !NETCF_35 && !MONO
    using Ninject.Extensions.Factory;
#endif

    /// <summary>
    /// The syntax to configure the conventions
    /// </summary>
    public partial class ConventionSyntax
    {
        /// <summary>
        /// Bind using a custom binding generator.
        /// </summary>
        /// <typeparam name="T">The type of the binding generator</typeparam>
        /// <returns>The fluent syntax</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Makes the API simpler.")]
        public IConfigureSyntax BindWith<T>() where T : IBindingGenerator, new()
        {
            return this.BindWith(new T());
        }

        /// <summary>
        /// Bind using a custom binding generator.
        /// </summary>
        /// <param name="generator">The generator used to create the bindings.</param>
        /// <returns>The fluent syntax</returns>
        public IConfigureSyntax BindWith(IBindingGenerator generator)
        {
            this.bindingBuilder.BindWith(generator);
            return this;
        }

        /// <summary>
        /// Binds all interfaces of the given types to the type.
        /// </summary>
        /// <returns>The fluent syntax</returns>
        public IConfigureSyntax BindAllInterfaces()
        {
            return this.BindWith(this.bindingGeneratorFactory.CreateAllInterfacesBindingGenerator());
        }

        /// <summary>
        /// Binds the base type of the given types to the type.
        /// </summary>
        /// <returns>The fluent syntax</returns>
        public IConfigureSyntax BindBase()
        {
            return this.BindWith(this.bindingGeneratorFactory.CreateBaseBindingGenerator());
        }

        /// <summary>
        /// Binds the default interface of the given types to the type.
        /// e.g. Foo : IFoo 
        /// </summary>
        /// <returns>The fluent syntax</returns>
        public IConfigureSyntax BindDefaultInterface()
        {
            return this.BindWith(this.bindingGeneratorFactory.CreateDefaultInterfaceBindingGenerator());
        }

        /// <summary>
        ///  Binds the default interface of the given types to the type.
        /// e.g. MyFoo matches IFoo, and SuperCrazyFoo matches ICrazyFoo and IFoo
        /// </summary>
        /// <returns>The fluent syntax</returns>
        public IConfigureSyntax BindDefaultInterfaces()
        {
            return this.BindWith(this.bindingGeneratorFactory.CreateDefaultInterfacesBindingGenerator());
        }

        /// <summary>
        /// Expects that the given type has a single interface.
        /// In this case the interface is bound to the type.
        /// </summary>
        /// <returns>The fluent syntax</returns>
        public IConfigureSyntax BindSingleInterface()
        {
            return this.BindWith(this.bindingGeneratorFactory.CreateSingleInterfaceBindingGenerator());
        }

        /// <summary>
        /// Binds the type to itself.
        /// </summary>
        /// <returns>The fluent syntax</returns>
        public IConfigureSyntax BindToSelf()
        {
            return this.BindWith(this.bindingGeneratorFactory.CreateSelfBindingGenerator());
        }

        /// <summary>
        /// Binds the selected interfaces to the type.
        /// </summary>
        /// <param name="selector">The selector of the interfaces.</param>
        /// <returns>The fluent syntax</returns>
        public IConfigureSyntax BindSelection(ServiceSelector selector)
        {
            return this.BindWith(this.bindingGeneratorFactory.CreateSelectorBindingGenerator(selector));
        }

        /// <summary>
        /// Bind the type to its interfaces matching the given regular expression.
        /// </summary>
        /// <param name="pattern">The regular expression.</param>
        /// <returns>The fluent syntax</returns>
        public IConfigureSyntax BindUsingRegex(string pattern)
        {
            return this.BindWith(this.bindingGeneratorFactory.CreateRegexBindingGenerator(pattern));
        }

        /// <summary>
        /// Bind the type to its interfaces matching the given regular expression.
        /// </summary>
        /// <param name="pattern">The regular expression.</param>
        /// <param name="options">The regex options.</param>
        /// <returns>The fluent syntax</returns>
        public IConfigureSyntax BindUsingRegex(string pattern, RegexOptions options)
        {
            return this.BindWith(this.bindingGeneratorFactory.CreateRegexBindingGenerator(pattern, options));
        }

#if !SILVERLIGHT_20 && !WINDOWS_PHONE && !NETCF_35 && !MONO
        /// <summary>
        /// Binds interfaces to factory implementations using the factory extension.
        /// </summary>
        /// <returns>The fluent syntax</returns>
        public IConfigureSyntax BindToFactory()
        {
            return this.BindWith(this.bindingGeneratorFactory.FactoryBindingGenerator(null));
        }

        /// <summary>
        /// Binds interfaces to factory implementations using the factory extension.
        /// </summary>
        /// <param name="instanceProvider">The instance provider.</param>
        /// <returns>The fluent syntax</returns>
        public IConfigureSyntax BindToFactory(Func<IInstanceProvider> instanceProvider)
        {
            return this.BindWith(this.bindingGeneratorFactory.FactoryBindingGenerator(instanceProvider));
        }
#endif
    }
}