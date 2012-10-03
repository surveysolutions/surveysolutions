//-------------------------------------------------------------------------------
// <copyright file="ConventionSyntax.Select.cs" company="Ninject Project Contributors">
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
    using Ninject.Syntax;

    /// <summary>
    /// The syntax to configure the conventions
    /// </summary>
    public partial class ConventionSyntax : IFluentSyntax
    {
        /// <summary>
        /// Selects the types using the specified filter.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <returns>The fluent syntax</returns>
        public IJoinExcludeIncludeBindSyntax Select(Func<Type, bool> filter)
        {
            this.bindingBuilder.Where(filter);
            return this;
        }

        /// <summary>
        /// Selects all types.
        /// </summary>
        /// <returns>The fluent syntax</returns>
        public IJoinFilterWhereExcludeIncludeBindSyntax SelectAllTypes()
        {
            return this.SelectTypes(t => true);
        }

        /// <summary>
        /// Selects all none abstract classes.
        /// </summary>
        /// <returns>The fluent syntax</returns>
        public IJoinFilterWhereExcludeIncludeBindSyntax SelectAllClasses()
        {
            return this.SelectTypes(t => t.IsClass && !t.IsAbstract);
        }

        /// <summary>
        /// Selects all calsses including abstract ones.
        /// </summary>
        /// <returns>The fluent syntax</returns>
        public IJoinFilterWhereExcludeIncludeBindSyntax SelectAllIncludingAbstractClasses()
        {
            return this.SelectTypes(t => t.IsClass);
        }

        /// <summary>
        /// Selects all abstract classes.
        /// </summary>
        /// <returns>The fluent syntax</returns>
        public IJoinFilterWhereExcludeIncludeBindSyntax SelectAllAbstractClasses()
        {
            return this.SelectTypes(t => t.IsClass && t.IsAbstract);
        }

        /// <summary>
        /// Selects all interfaces.
        /// </summary>
        /// <returns>The fluent syntax</returns>
        public IJoinFilterWhereExcludeIncludeBindSyntax SelectAllInterfaces()
        {
            return this.SelectTypes(t => t.IsInterface);
        }
    }
}