//-------------------------------------------------------------------------------
// <copyright file="ConstructorInfoExtensions.cs" company="Ninject Project Contributors">
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
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Extensions for <see cref="ConstructorInfo"/>
    /// </summary>
    public static class ConstructorInfoExtensions
    {
        /// <summary>
        /// Gets the parameters with the specified type.
        /// </summary>
        /// <param name="constructorInfo">The constructor info.</param>
        /// <param name="parameterType">The requested type.</param>
        /// <returns>The parameters with the specified type.</returns>
        public static IEnumerable<ParameterInfo> GetParametersOfType(this ConstructorInfo constructorInfo, Type parameterType)
        {
            return constructorInfo.GetParameters().Where(argument => argument.ParameterType == parameterType);                
        }
    }
}