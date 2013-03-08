//-------------------------------------------------------------------------------
// <copyright file="ConventionSyntax.Filter.cs" company="Ninject Project Contributors">
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
    using System.Linq;
    using Ninject.Extensions.Conventions.Syntax;

    /// <summary>
    /// The syntax to configure the conventions
    /// </summary>
    public partial class ConventionSyntax
    {
        /// <summary>
        /// Selects the types in the specified namespaces.
        /// </summary>
        /// <param name="namespaces">The namespaces from which the types are selected.</param>
        /// <returns>The fluent syntax</returns>
        public IJoinFilterWhereExcludeIncludeBindSyntax InNamespaces(IEnumerable<string> namespaces)
        {
            return this.SelectTypes(t => namespaces.Any(ns => this.typeFilter.IsTypeInNamespace(t, ns)));
        }

        /// <summary>
        /// Selects the types in the specified namespaces.
        /// </summary>
        /// <param name="namespaces">The namespaces from which the types are selected.</param>
        /// <returns>The fluent syntax</returns>
        public IJoinFilterWhereExcludeIncludeBindSyntax InNamespaces(params string[] namespaces)
        {
            return this.InNamespaces(namespaces.AsEnumerable());
        }

        /// <summary>
        /// Selects the types in the same namespaces as the given types.
        /// </summary>
        /// <param name="types">The types defining the namespaces.</param>
        /// <returns>The fluent syntax</returns>
        public IJoinFilterWhereExcludeIncludeBindSyntax InNamespaceOf(params Type[] types)
        {
            return this.InNamespaces(types.Select(t => t.Namespace));
        }

        /// <summary>
        /// Selects the types in the same namespace as the given type.
        /// </summary>
        /// <typeparam name="T">The type defining the namespace.</typeparam>
        /// <returns>The fluent syntax</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Makes the API simpler.")]
        public IJoinFilterWhereExcludeIncludeBindSyntax InNamespaceOf<T>()
        {
            return this.InNamespaceOf(typeof(T));
        }

        /// <summary>
        /// Selects all types not in the given namespaces.
        /// </summary>
        /// <param name="namespaces">The namespaces from which the types are not selected.</param>
        /// <returns>The fluent syntax</returns>
        public IJoinFilterWhereExcludeIncludeBindSyntax NotInNamespaces(IEnumerable<string> namespaces)
        {
            return this.SelectTypes(t => namespaces.All(ns => !this.typeFilter.IsTypeInNamespace(t, ns)));
        }

        /// <summary>
        /// Selects all types not in the given namespaces.
        /// </summary>
        /// <param name="namespaces">The namespaces from which the types are not selected.</param>
        /// <returns>The fluent syntax</returns>
        public IJoinFilterWhereExcludeIncludeBindSyntax NotInNamespaces(params string[] namespaces)
        {
            return this.NotInNamespaces(namespaces.AsEnumerable());
        }

        /// <summary>
        /// Selects all types not in same namespaces as the given types.
        /// </summary>
        /// <param name="types">The types defining the namespace.</param>
        /// <returns>The fluent syntax</returns>
        public IJoinFilterWhereExcludeIncludeBindSyntax NotInNamespaceOf(params Type[] types)
        {
            return this.NotInNamespaces(types.Select(t => t.Namespace));
        }

        /// <summary>
        /// Selects all types not in same namespaces as the given type.
        /// </summary>
        /// <typeparam name="T">The type defining the namespace.</typeparam>
        /// <returns>The fluent syntax</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Makes the API simpler.")]
        public IJoinFilterWhereExcludeIncludeBindSyntax NotInNamespaceOf<T>()
        {
            return this.NotInNamespaceOf(typeof(T));
        }

        /// <summary>
        /// Selects the types inherited from any of the given types.
        /// </summary>
        /// <param name="types">The ancestor types.</param>
        /// <returns>The fluent syntax</returns>
        public IJoinFilterWhereExcludeIncludeBindSyntax InheritedFromAny(params Type[] types)
        {
            return this.InheritedFromAny(types.AsEnumerable());
        }

        /// <summary>
        /// Selects the types inherited from any of the given types.
        /// </summary>
        /// <param name="types">The ancestor types.</param>
        /// <returns>The fluent syntax</returns>
        public IJoinFilterWhereExcludeIncludeBindSyntax InheritedFromAny(IEnumerable<Type> types)
        {
            return this.SelectTypes(t => this.typeFilter.IsTypeInheritedFromAny(t, types));
        }

        /// <summary>
        /// Selects the types inherited from the given types.
        /// </summary>
        /// <param name="type">The ancestor type.</param>
        /// <returns>The fluent syntax</returns>
        public IJoinFilterWhereExcludeIncludeBindSyntax InheritedFrom(Type type)
        {
            return this.InheritedFromAny(type);
        }

        /// <summary>
        /// Selects the types inherited from the given types.
        /// </summary>
        /// <typeparam name="T">The ancestor type.</typeparam>
        /// <returns>The fluent syntax</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Makes the API simpler.")]
        public IJoinFilterWhereExcludeIncludeBindSyntax InheritedFrom<T>()
        {
            return this.InheritedFrom(typeof(T));
        }

        /// <summary>
        /// Selects the types with the specified attribute.
        /// </summary>
        /// <typeparam name="T">The type of the attribute</typeparam>
        /// <returns>The fluent syntax</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Makes the API simpler.")]
        public IJoinFilterWhereExcludeIncludeBindSyntax WithAttribute<T>() where T : Attribute
        {
            return this.WithAttribute(typeof(T));
        }

        /// <summary>
        /// Selects the types that matches the specified attribute predicate.
        /// </summary>
        /// <typeparam name="T">The type of the attribute</typeparam>
        /// <param name="predicate">A function to test if an attribute matches.</param>
        /// <returns>The fluent syntax</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Makes the API simpler.")]
        public IJoinFilterWhereExcludeIncludeBindSyntax WithAttribute<T>(Func<T, bool> predicate) where T : Attribute
        {
            return this.SelectTypes(t => this.typeFilter.HasAttribute(t, predicate));
        }

        /// <summary>
        /// Selects the types with the specified attribute.
        /// </summary>
        /// <param name="attributeType">The type of the attribute.</param>
        /// <returns>The fluent syntax</returns>
        public IJoinFilterWhereExcludeIncludeBindSyntax WithAttribute(Type attributeType)
        {
            return this.SelectTypes(t => this.typeFilter.HasAttribute(t, attributeType));
        }

        /// <summary>
        /// Selects the types without the specified attribute.
        /// </summary>
        /// <typeparam name="T">The type of the attribute</typeparam>
        /// <returns>The fluent syntax</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Makes the API simpler.")]
        public IJoinFilterWhereExcludeIncludeBindSyntax WithoutAttribute<T>() where T : Attribute
        {
            return this.WithoutAttribute(typeof(T));
        }

        /// <summary>
        /// Selects the types that do not match the specified attribute predicate.
        /// </summary>
        /// <typeparam name="T">The type of the attribute</typeparam>
        /// <param name="predicate">A function to test if an attribute matches.</param>
        /// <returns>The fluent syntax</returns>
        public IJoinFilterWhereExcludeIncludeBindSyntax WithoutAttribute<T>(Func<T, bool> predicate) where T : Attribute
        {
            return this.SelectTypes(t => !this.typeFilter.HasAttribute(t, predicate));
        }

        /// <summary>
        /// Selects the types without the specified attribute.
        /// </summary>
        /// <param name="attributeType">The type of the attribute.</param>
        /// <returns>The fluent syntax</returns>
        public IJoinFilterWhereExcludeIncludeBindSyntax WithoutAttribute(Type attributeType)
        {
            return this.SelectTypes(t => !this.typeFilter.HasAttribute(t, attributeType));
        }

        /// <summary>
        /// Selects the types that are generic.
        /// </summary>
        /// <returns>The fluent syntax</returns>
        public IJoinFilterWhereExcludeIncludeBindSyntax WhichAreGeneric()
        {
            return this.SelectTypes(t => t.IsGenericType);
        }

        /// <summary>
        /// Selects the types that are not generic.
        /// </summary>
        /// <returns>The fluent syntax</returns>
        public IJoinFilterWhereExcludeIncludeBindSyntax WhichAreNotGeneric()
        {
            return this.SelectTypes(t => !t.IsGenericType);
        }

        /// <summary>
        /// Selects all types that have the specified postfix.
        /// </summary>
        /// <param name="postfix">The postfix.</param>
        /// <returns>The fluent syntax</returns>
        public IJoinFilterWhereExcludeIncludeBindSyntax EndingWith(string postfix)
        {
            return this.SelectTypes(t => t.Name.EndsWith(postfix));
        }

        /// <summary>
        /// Selects all types that have the specified prefix.
        /// </summary>
        /// <param name="prefix">The prefix.</param>
        /// <returns>The fluent syntax</returns>
        public IJoinFilterWhereExcludeIncludeBindSyntax StartingWith(string prefix)
        {
            return this.SelectTypes(t => t.Name.StartsWith(prefix));
        }

    }
}