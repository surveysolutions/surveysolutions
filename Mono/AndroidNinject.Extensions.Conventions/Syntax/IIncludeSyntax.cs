//-------------------------------------------------------------------------------
// <copyright file="IIncludeSyntax.cs" company="Ninject Project Contributors">
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

namespace Ninject.Extensions.Conventions.Syntax
{
    using System;
    using System.Collections.Generic;

    using Ninject.Syntax;

    /// <summary>
    /// The fluent syntax to include some types
    /// </summary>
    public interface IIncludeSyntax : IFluentSyntax
    {
        /// <summary>
        /// Includes the given type
        /// </summary>
        /// <typeparam name="T">The type to be included</typeparam>
        /// <returns>The fluent syntax</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Makes the API simpler.")]
        IExcludeIncludeBindSyntax Including<T>();

        /// <summary>
        /// Includes the given types
        /// </summary>
        /// <param name="types">The types to be included.</param>
        /// <returns>The fluent syntax</returns>
        IExcludeIncludeBindSyntax Including(params Type[] types);

        /// <summary>
        /// Includes the given types
        /// </summary>
        /// <param name="types">The types to be included.</param>
        /// <returns>The fluent syntax</returns>
        IExcludeIncludeBindSyntax Including(IEnumerable<Type> types);     
    }
}