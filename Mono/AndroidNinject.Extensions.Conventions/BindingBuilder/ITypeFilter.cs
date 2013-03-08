//-------------------------------------------------------------------------------
// <copyright file="ITypeFilter.cs" company="Ninject Project Contributors">
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

    /// <summary>
    /// Decides if some criteria apply to a type.
    /// </summary>
    public interface ITypeFilter
    {
        /// <summary>
        /// Determines whether the type is in the given namespace.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="namespace">The name space.</param>
        /// <returns>
        /// <c>true</c> if the type is in the given namespace; otherwise, <c>false</c>.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "namespace",
            Justification = "No better alternative known.")]
        bool IsTypeInNamespace(Type type, string @namespace);

        /// <summary>
        /// Determines whether the type is inherited from any of the specified types.
        /// </summary>
        /// <param name="type">The checked type.</param>
        /// <param name="types">The possible base types.</param>
        /// <returns>
        ///     <c>true</c> if the given type is inherited from any of the specified types; otherwise, <c>false</c>.
        /// </returns>
        bool IsTypeInheritedFromAny(Type type, IEnumerable<Type> types);

        /// <summary>
        /// Determines whether the specified type has the specified attribute.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="attributeType">The type of the attribute.</param>
        /// <returns>
        ///     <c>true</c> if the specified type has the attribute; otherwise, <c>false</c>.
        /// </returns>
        bool HasAttribute(Type type, Type attributeType);

        /// <summary>
        /// Determines whether the specified type has an attribute matching the predicate.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <param name="type">The type.</param>
        /// <param name="predicate">A function to test if an attribute matches.</param>
        /// <returns>
        ///     <c>true</c> if the specified type has an attribute that matches the predicate; otherwise, <c>false</c>.
        /// </returns>
        bool HasAttribute<TAttribute>(Type type, Func<TAttribute, bool> predicate);
    }
}