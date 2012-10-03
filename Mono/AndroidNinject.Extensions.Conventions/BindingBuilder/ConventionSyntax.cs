//-------------------------------------------------------------------------------
// <copyright file="ConventionSyntax.cs" company="Ninject Project Contributors">
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
    using Ninject.Extensions.Conventions.Syntax;

    /// <summary>
    /// The syntax to configure the conventions
    /// </summary>
    public partial class ConventionSyntax : IJoinFilterWhereExcludeIncludeBindSyntax, IIncludingNonePublicTypesSelectSyntax
    {
        /// <summary>
        /// Builder for conventions
        /// </summary>
        private readonly IConventionBindingBuilder bindingBuilder;

#if !NO_ASSEMBLY_SCANNING
        /// <summary>
        /// Finder for assemblies.
        /// </summary>
        private readonly IAssemblyFinder assemblyFinder;
#endif

        /// <summary>
        /// Filter for types.
        /// </summary>
        private readonly ITypeFilter typeFilter;

        /// <summary>
        /// Factory to create binding generators.
        /// </summary>
        private readonly IBindingGeneratorFactory bindingGeneratorFactory;

#if !NO_ASSEMBLY_SCANNING
        /// <summary>
        /// Initializes a new instance of the <see cref="ConventionSyntax"/> class.
        /// </summary>
        /// <param name="bindingBuilder">The binding builder.</param>
        /// <param name="assemblyFinder">The assembly finder.</param>
        /// <param name="typeFilter">The type filter.</param>
        /// <param name="bindingGeneratorFactory">The binding generator factory.</param>
        public ConventionSyntax(
            IConventionBindingBuilder bindingBuilder, 
            IAssemblyFinder assemblyFinder,
            ITypeFilter typeFilter,
            IBindingGeneratorFactory bindingGeneratorFactory)
        {
            this.bindingBuilder = bindingBuilder;
            this.assemblyFinder = assemblyFinder;
            this.typeFilter = typeFilter;
            this.bindingGeneratorFactory = bindingGeneratorFactory;
        }
#else
        /// <summary>
        /// Initializes a new instance of the <see cref="ConventionSyntax"/> class.
        /// </summary>
        /// <param name="bindingBuilder">The binding builder.</param>
        /// <param name="typeFilter">The type filter.</param>
        /// <param name="bindingGeneratorFactory">The binding generator factory.</param>
        public ConventionSyntax(
            IConventionBindingBuilder bindingBuilder, 
            ITypeFilter typeFilter,
            IBindingGeneratorFactory bindingGeneratorFactory)
        {
            this.bindingBuilder = bindingBuilder;
            this.typeFilter = typeFilter;
            this.bindingGeneratorFactory = bindingGeneratorFactory;
        }
#endif

        /// <inheritdoc />
        public IFromSyntax Join
        {
            get
            {
                return this;
            }
        }
        
        /// <inheritdoc />
        public IJoinExcludeIncludeBindSyntax Where(Func<Type, bool> filter)
        {
            this.bindingBuilder.Where(filter);
            return this;
        }

#if !NO_SKIP_VISIBILITY
        /// <inheritdoc />
        public ISelectSyntax IncludingNonePublicTypes()
        {
            this.bindingBuilder.IncludingNonePublicTypes();
            return this;
        }
#endif

        private IJoinFilterWhereExcludeIncludeBindSyntax SelectTypes(Func<Type, bool> filter)
        {
            this.bindingBuilder.Where(filter);
            return this;
        }
    }
}