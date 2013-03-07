//-------------------------------------------------------------------------------
// <copyright file="TypeSelector.cs" company="bbv Software Services AG">
//   Copyright (c) 2010 bbv Software Services AG
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
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
    using System.Reflection;

    using Ninject.Extensions.Conventions.Extensions;

    /// <summary>
    /// Selects types from the given assemblies.
    /// </summary>
    public class TypeSelector : ITypeSelector
    {
        /// <summary>
        /// Gets all exported types from the specified assemblies.
        /// </summary>
        /// <param name="assemblies">The assemblies to search for types.</param>
        /// <returns>
        /// All exported types from the specified assemblies.
        /// </returns>
        public IEnumerable<Type> GetExportedTypes(IEnumerable<Assembly> assemblies)
        {
            return assemblies.SelectMany(asm => asm.GetExportedTypesPlatformSafe());
        }

#if !NO_SKIP_VISIBILITY
        /// <summary>
        /// Gets all types from the specified assemblies.
        /// </summary>
        /// <param name="assemblies">The assemblies to search for types.</param>
        /// <returns>All types from the specified assemblies.</returns>
        public IEnumerable<Type> GetAllTypes(IEnumerable<Assembly> assemblies)
        {
            return assemblies.SelectMany(asm => asm.GetTypes());
        }
#endif
    }
}