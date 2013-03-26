//-------------------------------------------------------------------------------
// <copyright file="ExtensionsForIKernel.cs" company="Ninject Project Contributors">
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

namespace Ninject.Extensions.Conventions
{
    using System;

    using Ninject.Extensions.Conventions.BindingBuilder;
    using Ninject.Extensions.Conventions.BindingGenerators;
    using Ninject.Extensions.Conventions.Syntax;
    using Ninject.Infrastructure;
    using Ninject.Modules;
    using Ninject.Syntax;

    /// <summary>
    /// Provides extensions for the IKernel interface
    /// </summary>
    public static class ExtensionsForIKernel
    {
        /// <summary>
        /// Creates bindings using conventions
        /// </summary>
        /// <param name="kernel">The kernel for which the bindings are created.</param>
        /// <param name="configure">The binding convention configuration.</param>
        public static void Bind(this IBindingRoot kernel, Action<IFromSyntax> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException("configure");
            } 

#if !NO_ASSEMBLY_SCANNING
            var assemblyNameRetriever = new AssemblyNameRetriever();
            try
            {
                var builder = new ConventionSyntax(
                    new ConventionBindingBuilder(kernel, new TypeSelector()),
                    new AssemblyFinder(assemblyNameRetriever),
                    new TypeFilter(),
                    new BindingGeneratorFactory(new BindableTypeSelector()));
                configure(builder);
            }
            finally
            {
                assemblyNameRetriever.Dispose();                
            }
#else
            var builder = new ConventionSyntax(
                new ConventionBindingBuilder(kernel, new TypeSelector()), 
                new TypeFilter(), 
                new BindingGeneratorFactory(new BindableTypeSelector()));
            configure(builder);
#endif
        }
    }
}