//-------------------------------------------------------------------------------
// <copyright file="IFunctionFactory.cs" company="Ninject Project Contributors">
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
    using System.Reflection;

    using Ninject.Syntax;

    /// <summary>
    /// Factory for Func
    /// </summary>
    public interface IFunctionFactory
    {
        /// <summary>
        /// Gets the method info of the create method with the specified number of generic arguments.
        /// </summary>
        /// <param name="genericArgumentCount">The generic argument count.</param>
        /// <returns>The method info of the create method with the specified number of generic arguments.</returns>
        MethodInfo GetMethodInfo(int genericArgumentCount);

        /// <summary>
        /// Creates a new Func that creates a new <typeparamref name="TService"/> instance using the specified resolution root. 
        /// </summary>
        /// <typeparam name="TService">The type of the created service.</typeparam>
        /// <param name="resolutionRoot">The resolution root.</param>
        /// <returns>The new instance of <typeparamref name="TService"/> created using the resolution root.</returns>
        Func<TService> Create<TService>(IResolutionRoot resolutionRoot);

        /// <summary>
        /// Creates a new Func that creates a new <typeparamref name="TService"/> instance using the specified resolution root.
        /// </summary>
        /// <typeparam name="TArg1">The type of the 1st argument.</typeparam>
        /// <typeparam name="TService">The type of the created service.</typeparam>
        /// <param name="resolutionRoot">The resolution root.</param>
        /// <returns>
        /// The new instance of <typeparamref name="TService"/> created using the resolution root.
        /// </returns>
        Func<TArg1, TService> Create<TArg1, TService>(IResolutionRoot resolutionRoot);

        /// <summary>
        /// Creates a new Func that creates a new <typeparamref name="TService"/> instance using the specified resolution root.
        /// </summary>
        /// <typeparam name="TArg1">The type of the 1st argument.</typeparam>
        /// <typeparam name="TArg2">The type of the 2nd argument.</typeparam>
        /// <typeparam name="TService">The type of the created service.</typeparam>
        /// <param name="resolutionRoot">The resolution root.</param>
        /// <returns>
        /// The new instance of <typeparamref name="TService"/> created using the resolution root.
        /// </returns>
        Func<TArg1, TArg2, TService> Create<TArg1, TArg2, TService>(IResolutionRoot resolutionRoot);

        /// <summary>
        /// Creates a new Func that creates a new <typeparamref name="TService"/> instance using the specified resolution root.
        /// </summary>
        /// <typeparam name="TArg1">The type of the 1st argument.</typeparam>
        /// <typeparam name="TArg2">The type of the 2nd argument.</typeparam>
        /// <typeparam name="TArg3">The type of the 3rd argument.</typeparam>
        /// <typeparam name="TService">The type of the created service.</typeparam>
        /// <param name="resolutionRoot">The resolution root.</param>
        /// <returns>
        /// The new instance of <typeparamref name="TService"/> created using the resolution root.
        /// </returns>
        Func<TArg1, TArg2, TArg3, TService> Create<TArg1, TArg2, TArg3, TService>(IResolutionRoot resolutionRoot);

        /// <summary>
        /// Creates a new Func that creates a new <typeparamref name="TService"/> instance using the specified resolution root.
        /// </summary>
        /// <typeparam name="TArg1">The type of the 1st argument.</typeparam>
        /// <typeparam name="TArg2">The type of the 2nd argument.</typeparam>
        /// <typeparam name="TArg3">The type of the 3rd argument.</typeparam>
        /// <typeparam name="TArg4">The type of the 4th argument.</typeparam>
        /// <typeparam name="TService">The type of the created service.</typeparam>
        /// <param name="resolutionRoot">The resolution root.</param>
        /// <returns>
        /// The new instance of <typeparamref name="TService"/> created using the resolution root.
        /// </returns>
        Func<TArg1, TArg2, TArg3, TArg4, TService>
            Create<TArg1, TArg2, TArg3, TArg4, TService>(IResolutionRoot resolutionRoot);

#if !NET_35 && !SILVERLIGHT_30 && !SILVERLIGHT_20 && !WINDOWS_PHONE && !NETCF_35
        /// <summary>
        /// Creates a new Func that creates a new <typeparamref name="TService"/> instance using the specified resolution root.
        /// </summary>
        /// <typeparam name="TArg1">The type of the 1st argument.</typeparam>
        /// <typeparam name="TArg2">The type of the 2nd argument.</typeparam>
        /// <typeparam name="TArg3">The type of the 3rd argument.</typeparam>
        /// <typeparam name="TArg4">The type of the 4th argument.</typeparam>
        /// <typeparam name="TArg5">The type of the 5th argument.</typeparam>
        /// <typeparam name="TService">The type of the created service.</typeparam>
        /// <param name="resolutionRoot">The resolution root.</param>
        /// <returns>
        /// The new instance of <typeparamref name="TService"/> created using the resolution root.
        /// </returns>
        Func<TArg1, TArg2, TArg3, TArg4, TArg5, TService>
            Create<TArg1, TArg2, TArg3, TArg4, TArg5, TService>(IResolutionRoot resolutionRoot);

        /// <summary>
        /// Creates a new Func that creates a new <typeparamref name="TService"/> instance using the specified resolution root.
        /// </summary>
        /// <typeparam name="TArg1">The type of the 1st argument.</typeparam>
        /// <typeparam name="TArg2">The type of the 2nd argument.</typeparam>
        /// <typeparam name="TArg3">The type of the 3rd argument.</typeparam>
        /// <typeparam name="TArg4">The type of the 4th argument.</typeparam>
        /// <typeparam name="TArg5">The type of the 5th argument.</typeparam>
        /// <typeparam name="TArg6">The type of the 6th argument.</typeparam>
        /// <typeparam name="TService">The type of the created service.</typeparam>
        /// <param name="resolutionRoot">The resolution root.</param>
        /// <returns>
        /// The new instance of <typeparamref name="TService"/> created using the resolution root.
        /// </returns>
        Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TService>
            Create<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TService>(IResolutionRoot resolutionRoot);

        /// <summary>
        /// Creates a new Func that creates a new <typeparamref name="TService"/> instance using the specified resolution root.
        /// </summary>
        /// <typeparam name="TArg1">The type of the 1st argument.</typeparam>
        /// <typeparam name="TArg2">The type of the 2nd argument.</typeparam>
        /// <typeparam name="TArg3">The type of the 3rd argument.</typeparam>
        /// <typeparam name="TArg4">The type of the 4th argument.</typeparam>
        /// <typeparam name="TArg5">The type of the 5th argument.</typeparam>
        /// <typeparam name="TArg6">The type of the 6th argument.</typeparam>
        /// <typeparam name="TArg7">The type of the 7th argument.</typeparam>
        /// <typeparam name="TService">The type of the created service.</typeparam>
        /// <param name="resolutionRoot">The resolution root.</param>
        /// <returns>
        /// The new instance of <typeparamref name="TService"/> created using the resolution root.
        /// </returns>
        Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TService>
            Create<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TService>(IResolutionRoot resolutionRoot);

        /// <summary>
        /// Creates a new Func that creates a new <typeparamref name="TService"/> instance using the specified resolution root.
        /// </summary>
        /// <typeparam name="TArg1">The type of the 1st argument.</typeparam>
        /// <typeparam name="TArg2">The type of the 2nd argument.</typeparam>
        /// <typeparam name="TArg3">The type of the 3rd argument.</typeparam>
        /// <typeparam name="TArg4">The type of the 4th argument.</typeparam>
        /// <typeparam name="TArg5">The type of the 5th argument.</typeparam>
        /// <typeparam name="TArg6">The type of the 6th argument.</typeparam>
        /// <typeparam name="TArg7">The type of the 7th argument.</typeparam>
        /// <typeparam name="TArg8">The type of the 8th argument.</typeparam>
        /// <typeparam name="TService">The type of the created service.</typeparam>
        /// <param name="resolutionRoot">The resolution root.</param>
        /// <returns>
        /// The new instance of <typeparamref name="TService"/> created using the resolution root.
        /// </returns>
        Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TService>
            Create<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TService>(IResolutionRoot resolutionRoot);

        /// <summary>
        /// Creates a new Func that creates a new <typeparamref name="TService"/> instance using the specified resolution root.
        /// </summary>
        /// <typeparam name="TArg1">The type of the 1st argument.</typeparam>
        /// <typeparam name="TArg2">The type of the 2nd argument.</typeparam>
        /// <typeparam name="TArg3">The type of the 3rd argument.</typeparam>
        /// <typeparam name="TArg4">The type of the 4th argument.</typeparam>
        /// <typeparam name="TArg5">The type of the 5th argument.</typeparam>
        /// <typeparam name="TArg6">The type of the 6th argument.</typeparam>
        /// <typeparam name="TArg7">The type of the 7th argument.</typeparam>
        /// <typeparam name="TArg8">The type of the 8th argument.</typeparam>
        /// <typeparam name="TArg9">The type of the 9th argument.</typeparam>
        /// <typeparam name="TService">The type of the created service.</typeparam>
        /// <param name="resolutionRoot">The resolution root.</param>
        /// <returns>
        /// The new instance of <typeparamref name="TService"/> created using the resolution root.
        /// </returns>
        Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TService>
            Create<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TService>(IResolutionRoot resolutionRoot);

        /// <summary>
        /// Creates a new Func that creates a new <typeparamref name="TService"/> instance using the specified resolution root.
        /// </summary>
        /// <typeparam name="TArg1">The type of the 1st argument.</typeparam>
        /// <typeparam name="TArg2">The type of the 2nd argument.</typeparam>
        /// <typeparam name="TArg3">The type of the 3rd argument.</typeparam>
        /// <typeparam name="TArg4">The type of the 4th argument.</typeparam>
        /// <typeparam name="TArg5">The type of the 5th argument.</typeparam>
        /// <typeparam name="TArg6">The type of the 6th argument.</typeparam>
        /// <typeparam name="TArg7">The type of the 7th argument.</typeparam>
        /// <typeparam name="TArg8">The type of the 8th argument.</typeparam>
        /// <typeparam name="TArg9">The type of the 9th argument.</typeparam>
        /// <typeparam name="TArg10">The type of the 10th argument.</typeparam>
        /// <typeparam name="TService">The type of the created service.</typeparam>
        /// <param name="resolutionRoot">The resolution root.</param>
        /// <returns>
        /// The new instance of <typeparamref name="TService"/> created using the resolution root.
        /// </returns>
        Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TService>
            Create<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TService>(IResolutionRoot resolutionRoot);

        /// <summary>
        /// Creates a new Func that creates a new <typeparamref name="TService"/> instance using the specified resolution root.
        /// </summary>
        /// <typeparam name="TArg1">The type of the 1st argument.</typeparam>
        /// <typeparam name="TArg2">The type of the 2nd argument.</typeparam>
        /// <typeparam name="TArg3">The type of the 3rd argument.</typeparam>
        /// <typeparam name="TArg4">The type of the 4th argument.</typeparam>
        /// <typeparam name="TArg5">The type of the 5th argument.</typeparam>
        /// <typeparam name="TArg6">The type of the 6th argument.</typeparam>
        /// <typeparam name="TArg7">The type of the 7th argument.</typeparam>
        /// <typeparam name="TArg8">The type of the 8th argument.</typeparam>
        /// <typeparam name="TArg9">The type of the 9th argument.</typeparam>
        /// <typeparam name="TArg10">The type of the 10th argument.</typeparam>
        /// <typeparam name="TArg11">The type of the 11th argument.</typeparam>
        /// <typeparam name="TService">The type of the created service.</typeparam>
        /// <param name="resolutionRoot">The resolution root.</param>
        /// <returns>
        /// The new instance of <typeparamref name="TService"/> created using the resolution root.
        /// </returns>
        Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TService>
            Create<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TService>(IResolutionRoot resolutionRoot);

        /// <summary>
        /// Creates a new Func that creates a new <typeparamref name="TService"/> instance using the specified resolution root.
        /// </summary>
        /// <typeparam name="TArg1">The type of the 1st argument.</typeparam>
        /// <typeparam name="TArg2">The type of the 2nd argument.</typeparam>
        /// <typeparam name="TArg3">The type of the 3rd argument.</typeparam>
        /// <typeparam name="TArg4">The type of the 4th argument.</typeparam>
        /// <typeparam name="TArg5">The type of the 5th argument.</typeparam>
        /// <typeparam name="TArg6">The type of the 6th argument.</typeparam>
        /// <typeparam name="TArg7">The type of the 7th argument.</typeparam>
        /// <typeparam name="TArg8">The type of the 8th argument.</typeparam>
        /// <typeparam name="TArg9">The type of the 9th argument.</typeparam>
        /// <typeparam name="TArg10">The type of the 10th argument.</typeparam>
        /// <typeparam name="TArg11">The type of the 11th argument.</typeparam>
        /// <typeparam name="TArg12">The type of the 12th argument.</typeparam>
        /// <typeparam name="TService">The type of the created service.</typeparam>
        /// <param name="resolutionRoot">The resolution root.</param>
        /// <returns>
        /// The new instance of <typeparamref name="TService"/> created using the resolution root.
        /// </returns>
        Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TService>
            Create<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TService>(IResolutionRoot resolutionRoot);

        /// <summary>
        /// Creates a new Func that creates a new <typeparamref name="TService"/> instance using the specified resolution root.
        /// </summary>
        /// <typeparam name="TArg1">The type of the 1st argument.</typeparam>
        /// <typeparam name="TArg2">The type of the 2nd argument.</typeparam>
        /// <typeparam name="TArg3">The type of the 3rd argument.</typeparam>
        /// <typeparam name="TArg4">The type of the 4th argument.</typeparam>
        /// <typeparam name="TArg5">The type of the 5th argument.</typeparam>
        /// <typeparam name="TArg6">The type of the 6th argument.</typeparam>
        /// <typeparam name="TArg7">The type of the 7th argument.</typeparam>
        /// <typeparam name="TArg8">The type of the 8th argument.</typeparam>
        /// <typeparam name="TArg9">The type of the 9th argument.</typeparam>
        /// <typeparam name="TArg10">The type of the 10th argument.</typeparam>
        /// <typeparam name="TArg11">The type of the 11th argument.</typeparam>
        /// <typeparam name="TArg12">The type of the 12th argument.</typeparam>
        /// <typeparam name="TArg13">The type of the 13th argument.</typeparam>
        /// <typeparam name="TService">The type of the created service.</typeparam>
        /// <param name="resolutionRoot">The resolution root.</param>
        /// <returns>
        /// The new instance of <typeparamref name="TService"/> created using the resolution root.
        /// </returns>
        Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TService>
            Create<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TService>(IResolutionRoot resolutionRoot);

        /// <summary>
        /// Creates a new Func that creates a new <typeparamref name="TService"/> instance using the specified resolution root.
        /// </summary>
        /// <typeparam name="TArg1">The type of the 1st argument.</typeparam>
        /// <typeparam name="TArg2">The type of the 2nd argument.</typeparam>
        /// <typeparam name="TArg3">The type of the 3rd argument.</typeparam>
        /// <typeparam name="TArg4">The type of the 4th argument.</typeparam>
        /// <typeparam name="TArg5">The type of the 5th argument.</typeparam>
        /// <typeparam name="TArg6">The type of the 6th argument.</typeparam>
        /// <typeparam name="TArg7">The type of the 7th argument.</typeparam>
        /// <typeparam name="TArg8">The type of the 8th argument.</typeparam>
        /// <typeparam name="TArg9">The type of the 9th argument.</typeparam>
        /// <typeparam name="TArg10">The type of the 10th argument.</typeparam>
        /// <typeparam name="TArg11">The type of the 11th argument.</typeparam>
        /// <typeparam name="TArg12">The type of the 12th argument.</typeparam>
        /// <typeparam name="TArg13">The type of the 13th argument.</typeparam>
        /// <typeparam name="TArg14">The type of the 14th argument.</typeparam>
        /// <typeparam name="TService">The type of the created service.</typeparam>
        /// <param name="resolutionRoot">The resolution root.</param>
        /// <returns>
        /// The new instance of <typeparamref name="TService"/> created using the resolution root.
        /// </returns>
        Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TService>
            Create<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TService>(IResolutionRoot resolutionRoot);

        /// <summary>
        /// Creates a new Func that creates a new <typeparamref name="TService"/> instance using the specified resolution root.
        /// </summary>
        /// <typeparam name="TArg1">The type of the 1st argument.</typeparam>
        /// <typeparam name="TArg2">The type of the 2nd argument.</typeparam>
        /// <typeparam name="TArg3">The type of the 3rd argument.</typeparam>
        /// <typeparam name="TArg4">The type of the 4th argument.</typeparam>
        /// <typeparam name="TArg5">The type of the 5th argument.</typeparam>
        /// <typeparam name="TArg6">The type of the 6th argument.</typeparam>
        /// <typeparam name="TArg7">The type of the 7th argument.</typeparam>
        /// <typeparam name="TArg8">The type of the 8th argument.</typeparam>
        /// <typeparam name="TArg9">The type of the 9th argument.</typeparam>
        /// <typeparam name="TArg10">The type of the 10th argument.</typeparam>
        /// <typeparam name="TArg11">The type of the 11th argument.</typeparam>
        /// <typeparam name="TArg12">The type of the 12th argument.</typeparam>
        /// <typeparam name="TArg13">The type of the 13th argument.</typeparam>
        /// <typeparam name="TArg14">The type of the 14th argument.</typeparam>
        /// <typeparam name="TArg15">The type of the 15th argument.</typeparam>
        /// <typeparam name="TService">The type of the created service.</typeparam>
        /// <param name="resolutionRoot">The resolution root.</param>
        /// <returns>
        /// The new instance of <typeparamref name="TService"/> created using the resolution root.
        /// </returns>
        Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TService>
            Create<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TService>(IResolutionRoot resolutionRoot);

        /// <summary>
        /// Creates a new Func that creates a new <typeparamref name="TService"/> instance using the specified resolution root.
        /// </summary>
        /// <typeparam name="TArg1">The type of the 1st argument.</typeparam>
        /// <typeparam name="TArg2">The type of the 2nd argument.</typeparam>
        /// <typeparam name="TArg3">The type of the 3rd argument.</typeparam>
        /// <typeparam name="TArg4">The type of the 4th argument.</typeparam>
        /// <typeparam name="TArg5">The type of the 5th argument.</typeparam>
        /// <typeparam name="TArg6">The type of the 6th argument.</typeparam>
        /// <typeparam name="TArg7">The type of the 7th argument.</typeparam>
        /// <typeparam name="TArg8">The type of the 8th argument.</typeparam>
        /// <typeparam name="TArg9">The type of the 9th argument.</typeparam>
        /// <typeparam name="TArg10">The type of the 10th argument.</typeparam>
        /// <typeparam name="TArg11">The type of the 11th argument.</typeparam>
        /// <typeparam name="TArg12">The type of the 12th argument.</typeparam>
        /// <typeparam name="TArg13">The type of the 13th argument.</typeparam>
        /// <typeparam name="TArg14">The type of the 14th argument.</typeparam>
        /// <typeparam name="TArg15">The type of the 15th argument.</typeparam>
        /// <typeparam name="TArg16">The type of the 16th argument.</typeparam>
        /// <typeparam name="TService">The type of the created service.</typeparam>
        /// <param name="resolutionRoot">The resolution root.</param>
        /// <returns>
        /// The new instance of <typeparamref name="TService"/> created using the resolution root.
        /// </returns>
        Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16, TService> 
            Create<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16, TService>(IResolutionRoot resolutionRoot);
#endif
    }
}