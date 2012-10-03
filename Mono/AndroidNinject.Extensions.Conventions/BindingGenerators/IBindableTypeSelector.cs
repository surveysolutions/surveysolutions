//-------------------------------------------------------------------------------
// <copyright file="IBindableTypeSelector.cs" company="Ninject Project Contributors">
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

    /// <summary>
    /// Returns the interfaces and base types for which a binding to the given type can be created.
    /// </summary>
    public interface IBindableTypeSelector
    {
        /// <summary>
        /// Gets interfaces for which a binding can be created.
        /// e.g. an open generic type requires an open generic interface.
        /// </summary>
        /// <param name="type">The type for which the bindable interfaces shall be returned.</param>
        /// <returns>The interfaces for which a binding to the given type can be created.</returns>
        IEnumerable<Type> GetBindableInterfaces(Type type);

        /// <summary>
        /// Gets base types for which a binding can be created.
        /// e.g. an open generic type requires an open generic base type.
        /// </summary>
        /// <param name="type">The type for which the bindable base types shall be returned.</param>
        /// <returns>The base types for which a binding to the given type can be created.</returns>
        IEnumerable<Type> GetBindableBaseTypes(Type type);
    }
}