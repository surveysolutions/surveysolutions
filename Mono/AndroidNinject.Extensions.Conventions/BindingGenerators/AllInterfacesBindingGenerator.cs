//-------------------------------------------------------------------------------
// <copyright file="AllInterfacesBindingGenerator.cs" company="Ninject Project Contributors">
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
    using Ninject.Extensions.Conventions.BindingBuilder;

    /// <summary>
    /// Creates bindings for all interfaces of a type.
    /// </summary>
    public class AllInterfacesBindingGenerator : AbstractInterfaceBindingGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AllInterfacesBindingGenerator"/> class.
        /// </summary>
        /// <param name="bindableTypeSelector">The bindable type selector.</param>
        /// <param name="bindingCreator">Creator for multiple bindins for one type.</param>
        public AllInterfacesBindingGenerator(IBindableTypeSelector bindableTypeSelector, IBindingCreator bindingCreator)
            : base(bindableTypeSelector, bindingCreator, (ts, ti) => true)
        {
        }
    }
}