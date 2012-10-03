//-------------------------------------------------------------------------------
// <copyright file="FactoryInterceptor.cs" company="Ninject Project Contributors">
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

#if !SILVERLIGHT_20 && !WINDOWS_PHONE && !NETCF_35 && !MONO
namespace Ninject.Extensions.Factory
{
    using Castle.DynamicProxy;

    using Ninject.Extensions.Factory.Factory;
    using Ninject.Syntax;

    /// <summary>
    /// Interceptor called by the factory proxies
    /// </summary>
    public class FactoryInterceptor : InstanceResolver, IInterceptor
    {
        /// <summary>
        /// The instance provider.
        /// </summary>
        private readonly IInstanceProvider instanceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="FactoryInterceptor"/> class.
        /// </summary>
        /// <param name="resolutionRoot">The resolution root used to create new instances for the factory.</param>
        /// <param name="instanceProvider">The instance provider.</param>
        public FactoryInterceptor(IResolutionRoot resolutionRoot, IInstanceProvider instanceProvider)
            : base(resolutionRoot)
        {
            this.instanceProvider = instanceProvider;
        }

        /// <summary>
        /// Intercepts the specified invocation.
        /// </summary>
        /// <param name="invocation">The invocation.</param>
        public void Intercept(IInvocation invocation)
        {
            invocation.ReturnValue = this.instanceProvider.GetInstance(this, invocation.Method, invocation.Arguments);
        }
    }
}
#endif