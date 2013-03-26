//-------------------------------------------------------------------------------
// <copyright file="ConventionSyntax.From.cs" company="Ninject Project Contributors">
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
    using System.Reflection;
    using Ninject.Extensions.Conventions.Syntax;

    /// <summary>
    /// The syntax to configure the conventions
    /// </summary>
    public partial class ConventionSyntax : IFromSyntax
    {
        /// <summary>
        /// Scans the specified assemblies.
        /// </summary>
        /// <param name="assemblies">The assemblies.</param>
        /// <returns>The fluent syntax.</returns>
        public IIncludingNonePublicTypesSelectSyntax From(IEnumerable<Assembly> assemblies)
        {
            this.bindingBuilder.SelectAllTypesFrom(assemblies);
            return this;
        }

        /// <summary>
        /// Scans the specified assemblies.
        /// </summary>
        /// <param name="assemblies">The assemblies.</param>
        /// <returns>The fluent syntax.</returns>
        public IIncludingNonePublicTypesSelectSyntax From(params Assembly[] assemblies)
        {
            return this.From(assemblies.AsEnumerable());
        }

        /// <summary>
        /// Scans the calling assembly.
        /// </summary>
        /// <returns>The fluent syntax.</returns>
        public IIncludingNonePublicTypesSelectSyntax FromThisAssembly()
        {
            return this.From(Assembly.GetCallingAssembly());
        }

        /// <summary>
        /// Scans the assembly containing the specified type.
        /// </summary>
        /// <typeparam name="T">The type that specifies the assembly.</typeparam>
        /// <returns>The fluent syntax.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Makes the API simpler.")]
        public IIncludingNonePublicTypesSelectSyntax FromAssemblyContaining<T>()
        {
            return this.FromAssemblyContaining(typeof(T));
        }

        /// <summary>
        /// Scans the assembly containing the specified type..
        /// </summary>
        /// <param name="types">The types that specify the assemblies.</param>
        /// <returns>The fluent syntax.</returns>
        public IIncludingNonePublicTypesSelectSyntax FromAssemblyContaining(params Type[] types)
        {
            return this.FromAssemblyContaining(types.AsEnumerable());
        }

        /// <summary>
        /// Scans the assembly containing the specified type..
        /// </summary>
        /// <param name="types">The types that specify the assemblies.</param>
        /// <returns>The fluent syntax.</returns>
        public IIncludingNonePublicTypesSelectSyntax FromAssemblyContaining(IEnumerable<Type> types)
        {
            return this.From(types.Select(t => t.Assembly).Distinct());
        }

#if !NO_ASSEMBLY_SCANNING
        /// <summary>
        /// Scans the specified assemblies.
        /// </summary>
        /// <param name="assemblies">The name of the assemblies to be scanned.</param>
        /// <returns>The fluent syntax.</returns>
        public IIncludingNonePublicTypesSelectSyntax From(params string[] assemblies)
        {
            return this.From(assemblies.AsEnumerable());
        }

        /// <summary>
        /// Scans the specified assemblies.
        /// </summary>
        /// <param name="assemblies">The name of the assemblies to be scanned.</param>
        /// <returns>The fluent syntax.</returns>
        public IIncludingNonePublicTypesSelectSyntax From(IEnumerable<string> assemblies)
        {
            return From(assemblies, filter => true);
        }

        /// <summary>
        /// Scans the specified assemblies.
        /// </summary>
        /// <param name="assemblies">The name of the assemblies to be scanned.</param>
        /// <param name="filter">The filter for filtering the assemblies.</param>
        /// <returns>The fluent syntax.</returns>
        public IIncludingNonePublicTypesSelectSyntax From(IEnumerable<string> assemblies, Predicate<Assembly> filter)
        {
            return this.From(this.assemblyFinder.FindAssemblies(assemblies, filter));
        }

        /// <summary>
        /// Scans the assemblies in the given path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The fluent syntax.</returns>
        public IIncludingNonePublicTypesSelectSyntax FromAssembliesInPath(string path)
        {
            return this.FromAssembliesInPath(path, filter => true);
        }

        /// <summary>
        /// Scans the assemblies in the given path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="filter">The filter used to filter the assemblies.</param>
        /// <returns>The fluent syntax.</returns>
        public IIncludingNonePublicTypesSelectSyntax FromAssembliesInPath(string path, Predicate<Assembly> filter)
        {
            return this.From(this.assemblyFinder.FindAssembliesInPath(path), filter);
        }

        /// <summary>
        /// Scans the assemblies matching the given pattern.
        /// </summary>
        /// <param name="patterns">The patterns to match the assemblies.</param>
        /// <returns>The fluent syntax.</returns>
        public IIncludingNonePublicTypesSelectSyntax FromAssembliesMatching(params string[] patterns)
        {
            return this.FromAssembliesMatching(patterns.AsEnumerable());
        }

        /// <summary>
        /// Scans the assemblies matching the given pattern.
        /// </summary>
        /// <param name="patterns">The patterns to match the assemblies.</param>
        /// <returns>The fluent syntax.</returns>
        public IIncludingNonePublicTypesSelectSyntax FromAssembliesMatching(IEnumerable<string> patterns)
        {
            return this.From(this.assemblyFinder.FindAssembliesMatching(patterns));
        }
#endif
    }
}