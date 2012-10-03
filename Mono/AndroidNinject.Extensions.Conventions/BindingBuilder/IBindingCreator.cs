//-------------------------------------------------------------------------------
// <copyright file="IBindingCreator.cs" company="Ninject Project Contributors">
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
    using System.Collections.Generic;

    using Ninject.Syntax;

    /// <summary>
    /// Creates bindings.
    /// </summary>
    public interface IBindingCreator
    {
        /// <summary>
        /// Creates the bindings for the specified services.
        /// </summary>
        /// <param name="bindingRoot">The binding root.</param>
        /// <param name="serviceTypes">The service types.</param>
        /// <param name="implementationType">The implementation type.</param>
        /// <returns>The syntax of the created bindings.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nesting is required because the binding syntax is generic and an enuerable of them is used here.")]
        IEnumerable<IBindingWhenInNamedWithOrOnSyntax<object>> CreateBindings(
            IBindingRoot bindingRoot,
            IEnumerable<Type> serviceTypes,
            Type implementationType);
    }
}