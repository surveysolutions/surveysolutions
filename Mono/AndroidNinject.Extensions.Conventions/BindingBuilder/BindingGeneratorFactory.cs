//-------------------------------------------------------------------------------
// <copyright file="BindingGeneratorFactory.cs" company="Ninject Project Contributors">
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
    using System.Linq;
    using System.Text.RegularExpressions;

    using Ninject.Components;
    using Ninject.Extensions.Conventions.BindingGenerators;
    using Ninject.Extensions.Conventions.Syntax;
#if !SILVERLIGHT_20 && !WINDOWS_PHONE && !NETCF_35 && !MONO
    using Ninject.Extensions.Factory;
#endif

    /// <summary>
    /// Factory for binding generators.
    /// </summary>
    public class BindingGeneratorFactory : IBindingGeneratorFactory
    {
        /// <summary>
        /// Evaluator for base types nad interfaces.
        /// </summary>
        private readonly IBindableTypeSelector bindableTypeSelector;

        /// <summary>
        /// Initializes a new instance of the <see cref="BindingGeneratorFactory"/> class.
        /// </summary>
        /// <param name="bindableTypeSelector">The bindable type selector.</param>
        public BindingGeneratorFactory(IBindableTypeSelector bindableTypeSelector)
        {
            this.bindableTypeSelector = bindableTypeSelector;
        }

        /// <summary>
        /// Creates a regex binding generator.
        /// </summary>
        /// <param name="pattern">The regex pattern.</param>
        /// <returns>The newly created generator.</returns>
        public IBindingGenerator CreateRegexBindingGenerator(string pattern)
        {
            return this.CreateRegexBindingGenerator(pattern, RegexOptions.None);
        }

        /// <summary>
        /// Creates a regex binding generator.
        /// </summary>
        /// <param name="pattern">The regex pattern.</param>
        /// <param name="options">The regex options.</param>
        /// <returns>The newly created generator.</returns>
        public IBindingGenerator CreateRegexBindingGenerator(string pattern, RegexOptions options)
        {
            var regex = new Regex(pattern, options);
            return new SelectorBindingGenerator(
                this.CreateSingleBindingCreator(), 
                (_, serviceType) => serviceType.Where(t => regex.IsMatch(t.Name)),
                this.bindableTypeSelector);
        }

        /// <summary>
        /// Creates an all interfaces binding generator.
        /// </summary>
        /// <returns>The newly created generator.</returns>
        public IBindingGenerator CreateAllInterfacesBindingGenerator()
        {
            return new AllInterfacesBindingGenerator(this.bindableTypeSelector, this.CreateSingleBindingCreator());
        }

        /// <summary>
        /// Creates a base binding generator.
        /// </summary>
        /// <returns>The newly created generator.</returns>
        public IBindingGenerator CreateBaseBindingGenerator()
        {
            return new BaseBindingGenerator();
        }

        /// <summary>
        /// Creates a default interface binding generator.
        /// </summary>
        /// <returns>The newly created generator.</returns>
        public IBindingGenerator CreateDefaultInterfaceBindingGenerator()
        {
            return new DefaultInterfaceBindingGenerator(this.bindableTypeSelector, this.CreateSingleBindingCreator());
        }

        /// <summary>
        /// Creates a default interfaces binding generator.
        /// </summary>
        /// <returns>The newly created generator.</returns>
        public IBindingGenerator CreateDefaultInterfacesBindingGenerator()
        {
            return new DefaultInterfacesBindingGenerator(this.bindableTypeSelector, this.CreateSingleBindingCreator());
        }

        /// <summary>
        /// Creates a single interface binding generator.
        /// </summary>
        /// <returns>The newly created generator.</returns>
        public IBindingGenerator CreateSingleInterfaceBindingGenerator()
        {
            return new SingleInterfaceBindingGenerator(this.bindableTypeSelector);
        }

        /// <summary>
        /// Creates a self binding generator.
        /// </summary>
        /// <returns>The newly created generator.</returns>
        public IBindingGenerator CreateSelfBindingGenerator()
        {
            return new SelfBindingGenerator();
        }

        /// <summary>
        /// Creates a selector binding generator.
        /// </summary>
        /// <param name="selector">The selector.</param>
        /// <returns>The newly created generator.</returns>
        public IBindingGenerator CreateSelectorBindingGenerator(ServiceSelector selector)
        {
            return new SelectorBindingGenerator(this.CreateSingleBindingCreator(), selector, this.bindableTypeSelector);
        }

#if !SILVERLIGHT_20 && !WINDOWS_PHONE && !NETCF_35 && !MONO
        /// <summary>
        /// Creates a new FactoryBindingGenerator instance.
        /// </summary>
        /// <param name="instanceProvider">The instance provider passed to the new instance.</param>
        /// <returns>The newly created FactoryBindingGenerator.</returns>
        public IBindingGenerator FactoryBindingGenerator(Func<IInstanceProvider> instanceProvider)
        {
            return new FactoryBindingGenerator(instanceProvider);
        }
#endif

        /// <summary>
        /// Creates the multi binding creator.
        /// </summary>
        /// <returns>The newly created IMultiBindingCreator</returns>
        protected virtual IBindingCreator CreateSingleBindingCreator()
        {
            return new SingleConfigurationBindingCreator();
        }
    }
}