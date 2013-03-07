//-------------------------------------------------------------------------------
// <copyright file="ArgumentPositionCalculator.cs" company="Ninject Project Contributors">
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
    using System.Linq;
    using System.Reflection;

    using Ninject.Activation;
    using Ninject.Parameters;
    using Ninject.Planning.Targets;

    /// <summary>
    /// Calculates the position of method arguments
    /// </summary>
    public class ArgumentPositionCalculator : IArgumentPositionCalculator
    {
        /// <summary>
        /// Gets the position of the specified <see cref="FuncConstructorArgument"/> relative to the
        /// other <see cref="FuncConstructorArgument"/> of the same type in the specified context.
        /// </summary>
        /// <param name="argument">The argument for which the position is calculated.</param>
        /// <param name="context">The context of the argument.</param>
        /// <param name="target">The target.</param>
        /// <returns>
        ///     -1 if the parameter does not exist in the context or if another constructor argument applies for the target.
        ///     Otherwise the position of the specified <see cref="FuncConstructorArgument"/> within the other <see cref="FuncConstructorArgument"/> 
        ///     of the same type contained in context.Parameters.
        /// </returns>
        public int GetPositionOfFuncConstructorArgument(FuncConstructorArgument argument, IContext context, ITarget target)
        {
            int currentPosition = 0;
            int position = -1;
            foreach (var constructorArgumentParameter in context.Parameters.OfType<IConstructorArgument>())
            {
                var funcArgumentParameter = constructorArgumentParameter as FuncConstructorArgument;
                if (funcArgumentParameter != null)
                {
                    if (ReferenceEquals(argument, funcArgumentParameter))
                    {
                        position = currentPosition;
                    }
                    else
                    {
                        if (funcArgumentParameter.ArgumentType == target.Type)
                        {
                            currentPosition++;
                        }
                    }
                }
                else
                {
                    if (constructorArgumentParameter.AppliesToTarget(context, target))
                    {
                        return -1;
                    }
                }
            }

            return position;
        }

        /// <summary>
        /// Gets the position of the parameter specified by the target relative to the other parameters of the same
        /// type of the method containing the target. Parameters that apply to other ConstructorArguments are ignored.
        /// </summary>
        /// <param name="context">The current context.</param>
        /// <param name="target">The target for which the position is calculated.</param>
        /// <returns>
        ///     -1 if the target is not found of the parameter applies to another constructor argument.
        ///     Otherwise the position of the target relative to the other parameters of the method that have the same type and
        ///     do not apply to another <see cref="ConstructorArgument"/>.
        /// </returns>
        public int GetTargetPosition(IContext context, ITarget target)
        {
            int targetPosition = 0;
            var constructorInfo = (ConstructorInfo)target.Member;

            foreach (var parameter in constructorInfo.GetParametersOfType(target.Type))
            {
                var newTarget = new ParameterTarget(constructorInfo, parameter);
                if (!CheckOtherConstructorArgumentApplies(context, newTarget))
                {
                    if (parameter.Name == target.Name)
                    {
                        return targetPosition;
                    }

                    targetPosition++;
                }
            }

            return -1;
        }

        /// <summary>
        /// Checks if an <see cref="IConstructorArgument"/> with another type than <see cref="FuncConstructorArgument"/> applies to the target.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="target">The target.</param>
        /// <returns>True if an <see cref="IConstructorArgument"/> with another type than <see cref="FuncConstructorArgument"/> applies to the target.</returns>
        private static bool CheckOtherConstructorArgumentApplies(IContext context, ParameterTarget target)
        {
            return context
                .Parameters
                .OfType<IConstructorArgument>()
                .Any(p => !(p is FuncConstructorArgument) && p.AppliesToTarget(context, target));
        } 
    }
}