//-------------------------------------------------------------------------------
// <copyright file="BindableTypeSelector.cs" company="Ninject Project Contributors">
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

    /// <summary>
    /// Returns the interfaces and base types for which a binding to the given type can be created.
    /// </summary>
    public class BindableTypeSelector : IBindableTypeSelector
    {
        /// <summary>
        /// Gets interfaces for which a binding can be created.
        /// e.g. an open generic type requires an open generic interface.
        /// </summary>
        /// <param name="type">The type for which the bindable interfaces shall be returned.</param>
        /// <returns>
        /// The interfaces for which a binding to the given type can be created.
        /// </returns>
        public IEnumerable<Type> GetBindableInterfaces(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            } 
            
            if (type.IsInterface || type.IsAbstract)
            {
                return Enumerable.Empty<Type>();
            } 
            
            return type.IsGenericTypeDefinition
                ? type.GetInterfaces().Where(i => IsEqualOpenGeneric(i, type))
                                      .Select(i => i.GetGenericTypeDefinition()) 
                : type.GetInterfaces();
        }

        /// <summary>
        /// Gets base types for which a binding can be created.
        /// e.g. an open generic type requires an open generic base type.
        /// </summary>
        /// <param name="type">The type for which the bindable base types shall be returned.</param>
        /// <returns>
        /// The base types for which a binding to the given type can be created.
        /// </returns>
        public IEnumerable<Type> GetBindableBaseTypes(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            } 
            
            if (type.IsInterface || type.IsAbstract)
            {
                return Enumerable.Empty<Type>();
            }

            return type.IsGenericType 
                ? GetMatchingGenericBaseClasses(type) 
                : GetAllBaseClasses(type);
        }

        /// <summary>
        /// Gets all base classes of a type.
        /// </summary>
        /// <param name="type">The type of which the base classes are returned.</param>
        /// <returns>All base classes of a type.</returns>
        private static IEnumerable<Type> GetAllBaseClasses(Type type)
        {
            IList<Type> result = new List<Type>();
            type = type.BaseType;
            while (type != null)
            {
                result.Add(type);
                type = type.BaseType;
            }

            return result;
        }

        /// <summary>
        /// Determines whether the given interface is a generic with the same generic parameters.
        /// </summary>
        /// <param name="i">The i.</param>
        /// <param name="type">The type.</param>
        /// <returns>
        ///     <c>true</c> if [is equal open generic] [the specified i]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsEqualOpenGeneric(Type i, Type type)
        {
#if !WINDOWS_PHONE
            return i.ContainsGenericParameters &&
                   i.GetGenericArguments().SequenceEqual(type.GetGenericArguments());
#else
            return i.ContainsGenericParameters &&
                   i.GetGenericArguments().Select(t => t.Name).SequenceEqual(type.GetGenericArguments().Select(t => t.Name));
#endif
        }
        
        /// <summary>
        /// Gets the base classes that have the same generic arguments as the given type.
        /// </summary>
        /// <param name="type">The type for which the base calsses are returned.</param>
        /// <returns>The base classes that have the same generic arguments as the given type.</returns>
        private static IEnumerable<Type> GetMatchingGenericBaseClasses(Type type)
        {
            IList<Type> result = new List<Type>();
            var baseType = type.BaseType;
            
            while (baseType != null && IsEqualOpenGeneric(type, baseType))
            {
                result.Add(baseType.GetGenericTypeDefinition());
                baseType = baseType.BaseType;
            }

            return result;
        }
    }
}