//-------------------------------------------------------------------------------
// <copyright file="FuncProvider.cs" company="Ninject Project Contributors">
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

namespace Ninject.Extensions.Factory
{
    using System;

    using Ninject.Activation;
    using Ninject.Syntax;

    /// <summary>
    /// Provider for Func's
    /// </summary>
    public class FuncProvider : IProvider
    {
        /// <summary>
        /// The factory to create func instances.
        /// </summary>
        private readonly IFunctionFactory functionFactory;

        /// <summary>
        /// The resolution root used to create new instances.
        /// </summary>
        private readonly Func<IContext, IResolutionRoot> resolutionRootRetriever;

        /// <summary>
        /// Initializes a new instance of the <see cref="FuncProvider"/> class.
        /// </summary>
        /// <param name="functionFactory">The function factory.</param>
        /// <param name="resolutionRootRetriever">Func to get the resolution root from a context.</param>
        public FuncProvider(IFunctionFactory functionFactory, Func<IContext, IResolutionRoot> resolutionRootRetriever)
        {
            this.functionFactory = functionFactory;
            this.resolutionRootRetriever = resolutionRootRetriever;
        }

        /// <summary>
        /// Gets the type (or prototype) of instances the provider creates.
        /// </summary>
        /// <value>The type (or prototype) of instances the provider creates.</value>
        public Type Type
        {
            get
            {
                return typeof(Func<>);
            }
        }
        
        /// <summary>
        /// Creates an instance within the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>The created instance.</returns>
        public object Create(IContext context)
        {
            var genericArguments = context.GenericArguments;
            var mi = this.functionFactory.GetMethodInfo(genericArguments.Length);
            var createMethod = mi.MakeGenericMethod(genericArguments);
            return createMethod.Invoke(this.functionFactory, new object[] { this.resolutionRootRetriever(context) });       
        }
    }
}