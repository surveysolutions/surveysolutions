﻿//-------------------------------------------------------------------------------
// <copyright file="IConfigureForSyntax.cs" company="bbv Software Services AG">
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

namespace Ninject.Extensions.Conventions.Syntax
{
    using System;

    using Ninject.Syntax;

    /// <summary>
    /// The action to perform to configure the bindings.
    /// </summary>
    /// <param name="syntax">The binding syntax.</param>
    public delegate void ConfigurationAction(IBindingWhenInNamedWithOrOnSyntax<object> syntax);

    /// <summary>
    /// The action to perform to confugure the bindings.
    /// </summary>
    /// <param name="syntax">The binding syntax.</param>
    /// <param name="serviceType">The type of the service.</param>
    public delegate void ConfigurationActionWithService(IBindingWhenInNamedWithOrOnSyntax<object> syntax, Type serviceType);

    /// <summary>
    /// The syntax to configure special instances
    /// </summary>
    public interface IConfigureForSyntax : IFluentSyntax
    {
        /// <summary>
        /// Configures the bindings with the specified configuration.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The fluent syntax.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Makes the API simpler.")]
        IConfigureForSyntax ConfigureFor<TService>(ConfigurationAction configuration);
    
        /// <summary>
        /// Configures the bindings with the specified configuration.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The fluent syntax.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Makes the API simpler.")]
        IConfigureForSyntax ConfigureFor<TService>(ConfigurationActionWithService configuration);
    }
}