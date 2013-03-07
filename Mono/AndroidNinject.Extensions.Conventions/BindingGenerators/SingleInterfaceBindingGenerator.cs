﻿//-------------------------------------------------------------------------------
// <copyright file="SingleInterfaceBindingGenerator.cs" company="Ninject Project Contributors">
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

namespace Ninject.Extensions.Conventions.BindingGenerators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Ninject.Syntax;

    /// <summary>
    /// Binds the type to its single interface.
    /// </summary>
    public class SingleInterfaceBindingGenerator : IBindingGenerator
    {
        /// <summary>
        /// Evaluates the bindabe types.
        /// </summary>
        private readonly IBindableTypeSelector bindableTypeSelector;

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleInterfaceBindingGenerator"/> class.
        /// </summary>
        /// <param name="bindableTypeSelector">The bindable type selector.</param>
        public SingleInterfaceBindingGenerator(IBindableTypeSelector bindableTypeSelector)
        {
            this.bindableTypeSelector = bindableTypeSelector;
        }

        /// <summary>
        /// Creates the bindings for a type.
        /// </summary>
        /// <param name="type">The type for which the bindings are created.</param>
        /// <param name="bindingRoot">The binding root that is used to create the bindings.</param>
        /// <returns>
        /// The syntaxes of the created bindings to configure more options.
        /// </returns>
        public IEnumerable<IBindingWhenInNamedWithOrOnSyntax<object>> CreateBindings(Type type, IBindingRoot bindingRoot)
        {
            if (bindingRoot == null)
            {
                throw new ArgumentNullException("bindingRoot");
            } 
            
            var singleInterface = this.bindableTypeSelector.GetBindableInterfaces(type).Single();
            return new[] { bindingRoot.Bind(singleInterface).To(type) };
        }
    }
}