//-------------------------------------------------------------------------------
// <copyright file="IInstanceProvider.cs" company="Ninject Project Contributors">
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
    using System.Reflection;
    using Ninject.Extensions.Factory.Factory;

    /// <summary>
    /// Provides instances to the interceptor.
    /// </summary>
    public interface IInstanceProvider
    {
        /// <summary>
        /// Gets an instance for the specified method and arguments.
        /// </summary>
        /// <param name="instanceResolver">The instance resolver.</param>
        /// <param name="methodInfo">The method info.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The newly created instance.</returns>
        object GetInstance(IInstanceResolver instanceResolver, MethodInfo methodInfo, object[] arguments);
    }
}