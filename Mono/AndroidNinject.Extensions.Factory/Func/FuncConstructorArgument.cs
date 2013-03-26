//-------------------------------------------------------------------------------
// <copyright file="FuncConstructorArgument.cs" company="Ninject Project Contributors">
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
    using Ninject.Parameters;
    using Ninject.Planning.Targets;

    /// <summary>
    /// The <see cref="IConstructorArgument"/> used to define constructor arguments for Func bindings.
    /// </summary>
    public class FuncConstructorArgument : IConstructorArgument
    {
        /// <summary>
        /// The value of the argument.
        /// </summary>
        private readonly object value;

        /// <summary>
        /// The argument position calculator.
        /// </summary>
        private readonly IArgumentPositionCalculator argumentPositionCalculator;

        /// <summary>
        /// Initializes a new instance of the <see cref="FuncConstructorArgument"/> class.
        /// </summary>
        /// <param name="argumentType">The type of the argument.</param>
        /// <param name="value">The value.</param>
        /// <param name="argumentPositionCalculator">The argument position calculator.</param>
        public FuncConstructorArgument(Type argumentType, object value, IArgumentPositionCalculator argumentPositionCalculator)
        {
            this.value = value;
            this.argumentPositionCalculator = argumentPositionCalculator;
            this.ArgumentType = argumentType;
        }

        /// <summary>
        /// Gets the type of the argument.
        /// </summary>
        /// <value>The type of the argument.</value>
        public Type ArgumentType { get; private set; }

        /// <summary>
        /// Gets the name of the parameter.
        /// </summary>
        /// <value>The name of the parameter.</value>
        public string Name
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the parameter should be inherited into child requests.
        /// </summary>
        /// <value>Always <c>false</c>.</value>
        public bool ShouldInherit
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Returns if the given parameter is equal to this instance.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>True if the given parameter is equal to this instance.</returns>
        public bool Equals(IParameter other)
        {
            return ReferenceEquals(this, other);
        }

        /// <summary>
        /// Gets the value for the parameter within the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="target">The target.</param>
        /// <returns>The value for the parameter.</returns>
        public object GetValue(IContext context, ITarget target)
        {
            return this.value;
        }

        /// <summary>
        /// Determines if the parameter applies to the given target.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="target">The target.</param>
        /// <returns>
        /// True if the parameter applies in the specified context to the specified target.
        /// </returns>
        /// <remarks>
        /// Only one parameter may return <c>true</c>.
        /// </remarks>
        public bool AppliesToTarget(IContext context, ITarget target)
        {
            if (target.Type != this.ArgumentType)
            {
                return false;
            }

            int position = this.argumentPositionCalculator.GetPositionOfFuncConstructorArgument(this, context, target);
            return position > -1 && this.argumentPositionCalculator.GetTargetPosition(context, target) == position;
        }       
    }
}