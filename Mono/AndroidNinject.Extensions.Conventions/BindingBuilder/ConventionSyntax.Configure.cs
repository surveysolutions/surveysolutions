//-------------------------------------------------------------------------------
// <copyright file="ConventionSyntax.Configure.cs" company="Ninject Project Contributors">
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
    using Ninject.Extensions.Conventions.Syntax;

    /// <summary>
    /// The syntax to configure the conventions
    /// </summary>
    public partial class ConventionSyntax : IConfigureSyntax
    {
        /// <summary>
        /// Configures the bindings with the specified configuration.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The fluent syntax.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Makes the API simpler.")]
        public IConfigureForSyntax ConfigureFor<TService>(ConfigurationAction configuration)
        {
            this.bindingBuilder.ConfigureFor<TService>(configuration);
            return this;
        }

        /// <summary>
        /// Configures the bindings with the specified configuration.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The fluent syntax.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Makes the API simpler.")]
        public IConfigureForSyntax ConfigureFor<TService>(ConfigurationActionWithService configuration)
        {
            this.bindingBuilder.ConfigureFor<TService>(configuration);
            return this;
        }

        /// <summary>
        /// Configures the bindings with the specified configuration.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The fluent syntax.</returns>
        public IConfigureForSyntax Configure(ConfigurationAction configuration)
        {
            this.bindingBuilder.Configure(configuration);
            return this;
        }

        /// <summary>
        /// Configures the bindings with the specified configuration.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The fluent syntax.</returns>
        public IConfigureForSyntax Configure(ConfigurationActionWithService configuration)
        {
            this.bindingBuilder.Configure(configuration);
            return this;
        }
    }
}