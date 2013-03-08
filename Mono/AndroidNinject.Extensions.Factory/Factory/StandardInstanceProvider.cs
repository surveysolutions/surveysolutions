//-------------------------------------------------------------------------------
// <copyright file="StandardInstanceProvider.cs" company="Ninject Project Contributors">
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
    using System.Reflection;

    using Ninject.Extensions.Factory.Factory;
    using Ninject.Parameters;
    using Ninject.Planning.Bindings;

    /// <summary>
    /// The standard implementation of the instance provider
    /// </summary>
    public class StandardInstanceProvider : IInstanceProvider
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance falls back to requesting instances without 
        /// name or constraint if none can be received otherwise.
        /// </summary>
        /// <value><c>true</c> if this instance shall fall back to requesting instances without 
        /// name or constraint if none can be received otherwise, otherwise false.</value>
        public bool Fallback { get; set; }

        /// <summary>
        /// Gets an instance for the specified method and arguments.
        /// </summary>
        /// <param name="instanceResolver">The instance resolver.</param>
        /// <param name="methodInfo">The method info of the method that was called on the factory.</param>
        /// <param name="arguments">The arguments that were passed to the factory.</param>
        /// <returns>The newly created instance.</returns>
        public virtual object GetInstance(IInstanceResolver instanceResolver, MethodInfo methodInfo, object[] arguments)
        {
            var constructorArguments = this.GetConstructorArguments(methodInfo, arguments);
            var name = this.GetName(methodInfo, arguments);
            var constraint = this.GetConstraint(methodInfo, arguments);
            var type = this.GetType(methodInfo, arguments);

            if (type.IsGenericType)
            {
                var genericType = type.GetGenericTypeDefinition();
                if (genericType == typeof(IEnumerable<>) || 
                    genericType == typeof(ICollection<>) || 
                    genericType == typeof(IList<>) || 
                    genericType == typeof(List<>))
                {
                    var argumentType = type.GetGenericArguments()[0];
                    return instanceResolver.GetAllAsList(argumentType, name, constraint, constructorArguments, this.Fallback);
                }
            }

            if (type.IsArray)
            {
                var argumentType = type.GetElementType();
                return instanceResolver.GetAllAsArray(argumentType, name, constraint, constructorArguments, this.Fallback);
            }

            return instanceResolver.Get(type, name, constraint, constructorArguments, this.Fallback);
        }

        /// <summary>
        /// Gets the constraint for the specified method and arguments.
        /// </summary>
        /// <param name="methodInfo">The method info of the method that was called on the factory.</param>
        /// <param name="arguments">The arguments passed to the factory.</param>
        /// <returns>The constraint that shall be used to receive an instance. Null if no constraint shall be used.</returns>
        protected virtual Func<IBindingMetadata, bool> GetConstraint(MethodInfo methodInfo, object[] arguments)
        {
            return null;
        }

        /// <summary>
        /// Gets the type that shall be created for the specified method and arguments.
        /// </summary>
        /// <param name="methodInfo">The method info of the method that was called on the factory.</param>
        /// <param name="arguments">The arguments that were passed to the factory.</param>
        /// <returns>The type that shall be created for the specified method and arguments.</returns>
        protected virtual Type GetType(MethodInfo methodInfo, object[] arguments)
        {
            return methodInfo.ReturnType;
        }

        /// <summary>
        /// Gets the name that shall be used to request an instance for the specified method and arguments. 
        /// Null if unnamed instances shall be requested.
        /// </summary>
        /// <param name="methodInfo">The method info of the method that was called on the factory.</param>
        /// <param name="arguments">The arguments that were passed to the factory.</param>
        /// <returns>The name that shall be used to request an instance for the specified method and arguments. 
        /// Null if unnamed instances shall be requested.</returns>
        protected virtual string GetName(MethodInfo methodInfo, object[] arguments)
        {
            return methodInfo.Name.StartsWith("Get") ? methodInfo.Name.Substring(3) : null;
        }

        /// <summary>
        /// Gets the constructor arguments that shall be passed with the instance request.
        /// </summary>
        /// <param name="methodInfo">The method info of the method that was called on the factory.</param>
        /// <param name="arguments">The arguments that were passed to the factory.</param>
        /// <returns>The constructor arguments that shall be passed with the instance request.</returns>
        protected virtual ConstructorArgument[] GetConstructorArguments(MethodInfo methodInfo, object[] arguments)
        {
            var parameters = methodInfo.GetParameters();
            var constructorArguments = new ConstructorArgument[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                constructorArguments[i] = new ConstructorArgument(parameters[i].Name, arguments[i]);
            }

            return constructorArguments;
        }
    }
}