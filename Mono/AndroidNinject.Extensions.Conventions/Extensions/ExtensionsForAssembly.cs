//-------------------------------------------------------------------------------
// <copyright file="ExtensionsForAssembly.cs" company="Ninject Project Contributors">
//   Copyright (c) 2009-2011 Ninject Project Contributors
//   Authors: Ian Davis (ian@innovatian.com)
//            Remo Gloor (remo.gloor@gmail.com)
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

namespace Ninject.Extensions.Conventions.Extensions
{
    using System;
    using System.Collections.Generic;
#if NETCF
    using System.Linq;
#endif
    using System.Reflection;

    /// <summary>
    /// The extensions for assembly.
    /// </summary>
    internal static class ExtensionsForAssembly
    {
        /// <summary>
        /// Gets all exported types of the specified assembly.
        /// Plattform independent.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>
        /// All exported types of the specified assembly.
        /// </returns>
        public static IEnumerable<Type> GetExportedTypesPlatformSafe(this Assembly assembly)
        {
#if NETCF
            return assembly.GetTypes().Where( type => type.IsPublic );
#else
            return assembly.GetExportedTypes();
#endif
        }
    }
}