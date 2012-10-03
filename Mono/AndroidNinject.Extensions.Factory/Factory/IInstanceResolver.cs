//-------------------------------------------------------------------------------
// <copyright file="IInstanceResolver.cs" company="Ninject Project Contributors">
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

namespace Ninject.Extensions.Factory.Factory
{
    using System;

    using Ninject.Parameters;
    using Ninject.Planning.Bindings;

    /// <summary>
    /// Resolves instances from the kernel.
    /// </summary>
    public interface IInstanceResolver
    {
        /// <summary>
        /// Gets an instance of the specified type.
        /// </summary>
        /// <param name="type">The type of the instance.</param>
        /// <param name="name">The name of the binding to use. If null the name is not used.</param>
        /// <param name="constraint">The constraint for the bindings. If null the constraint is not used.</param>
        /// <param name="constructorArguments">The constructor arguments.</param>
        /// <param name="fallback">if set to <c>true</c> the request fallsback to requesting instances without 
        /// name or constraint if no one can received otherwise.</param>
        /// <returns>An instance of the specified type.</returns>
        object Get(
            Type type,
            string name,
            Func<IBindingMetadata, bool> constraint,
            ConstructorArgument[] constructorArguments, 
            bool fallback);

        /// <summary>
        /// Gets all instances of the specified type as list.
        /// </summary>
        /// <param name="type">The type of the instance.</param>
        /// <param name="name">The name of the binding to use. If null the name is not used.</param>
        /// <param name="constraint">The constraint for the bindings. If null the constraint is not used.</param>
        /// <param name="constructorArguments">The constructor arguments.</param>
        /// <param name="fallback">if set to <c>true</c> the request fallsback to requesting instances without 
        /// name or constraint if no one can received otherwise.</param>
        /// <returns>An instance of the specified type.</returns>
        object GetAllAsList(
            Type type,
            string name,
            Func<IBindingMetadata, bool> constraint,
            ConstructorArgument[] constructorArguments,
            bool fallback);
    
        /// <summary>
        /// Gets all instances of the specified type as array.
        /// </summary>
        /// <param name="type">The type of the instance.</param>
        /// <param name="name">The name of the binding to use. If null the name is not used.</param>
        /// <param name="constraint">The constraint for the bindings. If null the constraint is not used.</param>
        /// <param name="constructorArguments">The constructor arguments.</param>
        /// <param name="fallback">if set to <c>true</c> the request fallsback to requesting instances without 
        /// name or constraint if no one can received otherwise.</param>
        /// <returns>An instance of the specified type.</returns>
        object GetAllAsArray(
            Type type,
            string name,
            Func<IBindingMetadata, bool> constraint,
            ConstructorArgument[] constructorArguments,
            bool fallback);
    }
}