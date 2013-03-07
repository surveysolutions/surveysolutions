//-------------------------------------------------------------------------------
// <copyright file="AssemblyFinder.cs" company="Ninject Project Contributors">
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

#if !NO_ASSEMBLY_SCANNING
namespace Ninject.Extensions.Conventions.BindingBuilder
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Ninject.Modules;

    /// <summary>
    /// Provides functionality to detect assemblies.
    /// </summary>
    public class AssemblyFinder : IAssemblyFinder
    {
        /// <summary>
        /// Retrieves the name of an assembly form its file name.
        /// </summary>
        private readonly IAssemblyNameRetriever assemblyNameRetriever;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyFinder"/> class.
        /// </summary>
        /// <param name="assemblyNameRetriever">The assembly name retriever.</param>
        public AssemblyFinder(IAssemblyNameRetriever assemblyNameRetriever)
        {
            this.assemblyNameRetriever = assemblyNameRetriever;
        }

        /// <summary>
        /// Finds assemblies using filenames or assembly names.
        /// </summary>
        /// <param name="assemblies">The assemblies to search.</param>
        /// <param name="filter">A filter to filter certain assemblies.</param>
        /// <returns>The matching assemblies.</returns>
        public IEnumerable<Assembly> FindAssemblies(IEnumerable<string> assemblies, Predicate<Assembly> filter)
        {
            return this.assemblyNameRetriever
                .GetAssemblyNames(assemblies, filter)
                .Select<AssemblyName, Assembly>(Assembly.Load);
        }

        /// <summary>
        /// Searches for assemblies in the given path.
        /// </summary>
        /// <param name="path">The path to search.</param>
        /// <returns>The names of the detected assemblies.</returns>
        public IEnumerable<string> FindAssembliesInPath(string path)
        {
            return Directory.GetFiles(path).Where(IsAssemblyFile);
        }

        /// <summary>
        /// Searches for assemblies that match one of the given pattern.
        /// </summary>
        /// <param name="patterns">The patterns.</param>
        /// <returns>The names of the detected assemblies.</returns>
        public IEnumerable<string> FindAssembliesMatching(IEnumerable<string> patterns)
        {
            return patterns.SelectMany<string, string>(GetFilesMatchingPattern).Where(IsAssemblyFile);
        }

        private static bool IsAssemblyFile(string fileName)
        {
            string extension = Path.GetExtension(fileName);
            return HasAssemblyExtension(extension);
        }

        private static bool HasAssemblyExtension(string extension)
        {
            const StringComparison Comparison = StringComparison.OrdinalIgnoreCase;
            return string.Equals(extension, ".dll", Comparison) ||
                   string.Equals(extension, ".exe", Comparison);
        }

        private static IEnumerable<string> GetFilesMatchingPattern(string pattern)
        {
            string path = NormalizePath(Path.GetDirectoryName(pattern));
            string glob = Path.GetFileName(pattern);

            return Directory.GetFiles(path, glob);
        }

        private static string NormalizePath(string path)
        {
            if (!Path.IsPathRooted(path))
            {
                path = Path.Combine(GetBaseDirectory(), path);
            }

            return Path.GetFullPath(path);
        }

        private static string GetBaseDirectory()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string searchPath = AppDomain.CurrentDomain.RelativeSearchPath;

            return string.IsNullOrEmpty(searchPath) ? baseDirectory : Path.Combine(baseDirectory, searchPath);
        }
    }
}
#endif