//-------------------------------------------------------------------------------
// <copyright file="FunctionFactory.cs" company="Ninject Project Contributors">
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

    using Ninject.Parameters;
    using Ninject.Syntax;

    /// <summary>
    /// Factory for Func
    /// </summary>
    public class FunctionFactory : IFunctionFactory
    {
        /// <summary>
        /// The method infos of the create methods.
        /// </summary>
        private readonly MethodInfo[] methodInfos;

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionFactory"/> class.
        /// </summary>
        public FunctionFactory()
        {
            this.methodInfos = this.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public).Where(mi => mi.Name == "Create").ToArray();
        }

        /// <summary>
        /// Gets the method info of the create method with the specified number of generic arguments.
        /// </summary>
        /// <param name="genericArgumentCount">The generic argument count.</param>
        /// <returns>
        /// The method info of the create method with the specified number of generic arguments.
        /// </returns>
        public MethodInfo GetMethodInfo(int genericArgumentCount)
        {
            return this.methodInfos.Single(mi => mi.GetGenericArguments().Length == genericArgumentCount);
        }
        
        /// <summary>
        /// Creates a new Func that creates a new <typeparamref name="TService"/> instance using the specified resolution root. 
        /// </summary>
        /// <typeparam name="TService">The type of the created service.</typeparam>
        /// <param name="resolutionRoot">The resolution root.</param>
        /// <returns>The new instance of <typeparamref name="TService"/> created using the resolution root.</returns>
        public Func<TService> Create<TService>(IResolutionRoot resolutionRoot)
        {
            Type type;
            var convertFunction = GetConvertionFunction<TService>(out type);
            return () => Get(resolutionRoot, convertFunction, type);
        }

        /// <summary>
        /// Creates a new Func that creates a new <typeparamref name="TService"/> instance using the specified resolution root.
        /// </summary>
        /// <typeparam name="TArg1">The type of the 1st argument.</typeparam>
        /// <typeparam name="TService">The type of the created service.</typeparam>
        /// <param name="resolutionRoot">The resolution root.</param>
        /// <returns>
        /// The new instance of <typeparamref name="TService"/> created using the resolution root.
        /// </returns>
        public Func<TArg1, TService> Create<TArg1, TService>(IResolutionRoot resolutionRoot)
        {
            Type type;
            var convertFunction = GetConvertionFunction<TService>(out type);
            return arg1 => Get(
                resolutionRoot,
                convertFunction,
                type,
                FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg1), arg1));
        }

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
        public Func<TArg1, TArg2, TService> Create<TArg1, TArg2, TService>(IResolutionRoot resolutionRoot)
        {
            Type type;
            var convertFunction = GetConvertionFunction<TService>(out type);
            return (arg1, arg2) => Get(
                resolutionRoot,
                convertFunction,
                type,
                FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg1), arg1),
                FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg2), arg2));
        }

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
        public Func<TArg1, TArg2, TArg3, TService> Create<TArg1, TArg2, TArg3, TService>(IResolutionRoot resolutionRoot)
        {
            Type type;
            var convertFunction = GetConvertionFunction<TService>(out type);
            return (arg1, arg2, arg3) => Get(
                resolutionRoot,
                convertFunction,
                type,
                FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg1), arg1),
                FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg2), arg2),
                FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg3), arg3));
        }

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
        public Func<TArg1, TArg2, TArg3, TArg4, TService>
            Create<TArg1, TArg2, TArg3, TArg4, TService>(IResolutionRoot resolutionRoot)
        {
            Type type;
            var convertFunction = GetConvertionFunction<TService>(out type);
            return (arg1, arg2, arg3, arg4) =>
                   Get(
                       resolutionRoot,
                       convertFunction,
                       type,
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg1), arg1),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg2), arg2),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg3), arg3),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg4), arg4));
        }

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
        public Func<TArg1, TArg2, TArg3, TArg4, TArg5, TService>
            Create<TArg1, TArg2, TArg3, TArg4, TArg5, TService>(IResolutionRoot resolutionRoot)
        {
            Type type;
            var convertFunction = GetConvertionFunction<TService>(out type);
            return (arg1, arg2, arg3, arg4, arg5) =>
                   Get(
                       resolutionRoot,
                       convertFunction,
                       type,
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg1), arg1),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg2), arg2),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg3), arg3),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg4), arg4),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg5), arg5));
        }

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
        public Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TService>
            Create<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TService>(IResolutionRoot resolutionRoot)
        {
            Type type;
            var convertFunction = GetConvertionFunction<TService>(out type);
            return (arg1, arg2, arg3, arg4, arg5, arg6) =>
                   Get(
                       resolutionRoot,
                       convertFunction,
                       type,
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg1), arg1),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg2), arg2),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg3), arg3),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg4), arg4),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg5), arg5),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg6), arg6));
        }

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
        public Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TService>
            Create<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TService>(IResolutionRoot resolutionRoot)
        {
            Type type;
            var convertFunction = GetConvertionFunction<TService>(out type);
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7) =>
                   Get(
                       resolutionRoot,
                       convertFunction,
                       type,
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg1), arg1),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg2), arg2),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg3), arg3),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg4), arg4),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg5), arg5),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg6), arg6),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg7), arg7));
        }

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
        public Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TService>
            Create<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TService>(IResolutionRoot resolutionRoot)
        {
            Type type;
            var convertFunction = GetConvertionFunction<TService>(out type);
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8) =>
                   Get(
                       resolutionRoot,
                       convertFunction,
                       type,
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg1), arg1),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg2), arg2),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg3), arg3),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg4), arg4),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg5), arg5),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg6), arg6),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg7), arg7),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg8), arg8));
        }

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
        public Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TService>
            Create<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TService>(IResolutionRoot resolutionRoot)
        {
            Type type;
            var convertFunction = GetConvertionFunction<TService>(out type);
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9) =>
                   Get(
                       resolutionRoot,
                       convertFunction,
                       type,
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg1), arg1),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg2), arg2),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg3), arg3),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg4), arg4),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg5), arg5),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg6), arg6),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg7), arg7),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg8), arg8),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg9), arg9));
        }

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
        public Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TService>
            Create<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TService>(IResolutionRoot resolutionRoot)
        {
            Type type;
            var convertFunction = GetConvertionFunction<TService>(out type);
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10) =>
                   Get(
                       resolutionRoot,
                       convertFunction,
                       type,
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg1), arg1),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg2), arg2),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg3), arg3),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg4), arg4),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg5), arg5),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg6), arg6),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg7), arg7),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg8), arg8),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg9), arg9),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg10), arg10));
        }

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
        public Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TService>
            Create<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TService>(IResolutionRoot resolutionRoot)
        {
            Type type;
            var convertFunction = GetConvertionFunction<TService>(out type);
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11) =>
                   Get(
                       resolutionRoot,
                       convertFunction,
                       type,
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg1), arg1),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg2), arg2),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg3), arg3),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg4), arg4),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg5), arg5),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg6), arg6),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg7), arg7),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg8), arg8),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg9), arg9),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg10), arg10),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg11), arg11));
        }

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
        public Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TService>
            Create<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TService>(IResolutionRoot resolutionRoot)
        {
            Type type;
            var convertFunction = GetConvertionFunction<TService>(out type);
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12) =>
                   Get(
                       resolutionRoot,
                       convertFunction,
                       type,
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg1), arg1),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg2), arg2),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg3), arg3),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg4), arg4),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg5), arg5),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg6), arg6),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg7), arg7),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg8), arg8),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg9), arg9),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg10), arg10),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg11), arg11),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg12), arg12));
        }

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
        public Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TService>
            Create<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TService>(IResolutionRoot resolutionRoot)
        {
            Type type;
            var convertFunction = GetConvertionFunction<TService>(out type);
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13) =>
                   Get(
                       resolutionRoot,
                       convertFunction,
                       type,
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg1), arg1),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg2), arg2),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg3), arg3),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg4), arg4),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg5), arg5),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg6), arg6),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg7), arg7),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg8), arg8),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg9), arg9),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg10), arg10),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg11), arg11),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg12), arg12),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg13), arg13));
        }

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
        public Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TService>
            Create<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TService>(IResolutionRoot resolutionRoot)
        {
            Type type;
            var convertFunction = GetConvertionFunction<TService>(out type);
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14) =>
                   Get(
                       resolutionRoot,
                       convertFunction,
                       type,
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg1), arg1),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg2), arg2),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg3), arg3),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg4), arg4),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg5), arg5),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg6), arg6),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg7), arg7),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg8), arg8),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg9), arg9),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg10), arg10),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg11), arg11),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg12), arg12),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg13), arg13),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg14), arg14));
        }

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
        public Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TService>
            Create<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TService>(IResolutionRoot resolutionRoot)
        {
            Type type;
            var convertFunction = GetConvertionFunction<TService>(out type);
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15) =>
                   Get(
                       resolutionRoot,
                       convertFunction,
                       type,
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg1), arg1),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg2), arg2),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg3), arg3),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg4), arg4),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg5), arg5),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg6), arg6),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg7), arg7),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg8), arg8),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg9), arg9),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg10), arg10),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg11), arg11),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg12), arg12),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg13), arg13),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg14), arg14),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg15), arg15));
        }

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
        public Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16, TService> 
            Create<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16, TService>(IResolutionRoot resolutionRoot)
        {
            Type type;
            var convertFunction = GetConvertionFunction<TService>(out type);
            return (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16) => 
                   Get(
                       resolutionRoot,
                       convertFunction,
                       type,
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg1), arg1),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg2), arg2),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg3), arg3),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg4), arg4),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg5), arg5),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg6), arg6),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg7), arg7),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg8), arg8),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg9), arg9),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg10), arg10),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg11), arg11),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg12), arg12),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg13), arg13),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg14), arg14),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg15), arg15),
                       FuncConstructorArgumentFactory.CreateFuncConstructorArgument(typeof(TArg16), arg16));
        }
#endif

        /// <summary>
        /// Gets an instance using the specified resolution root.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <param name="resolutionRoot">The resolution root.</param>
        /// <param name="convertFunction">The convert function</param>
        /// <param name="type">The type to resolve.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The newly created instance.</returns>
        private static TService Get<TService>(
            IResolutionRoot resolutionRoot, 
            Func<IEnumerable<object>, TService> convertFunction,
            Type type,
            params IParameter[] parameters)
        {
            if (convertFunction != null)
            {
                return convertFunction(resolutionRoot.GetAll(type, parameters));
            }

            return resolutionRoot.Get<TService>(parameters);
        }

        /// <summary>
        /// Gets the convertion function to convert IEnumerable{object} to the result type.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <param name="instanceType">Type of the instance.</param>
        /// <returns>The convertion function.</returns>
        private static Func<IEnumerable<object>, TService> GetConvertionFunction<TService>(out Type instanceType)
        {
            var type = typeof(TService);
            if (type.IsGenericType)
            {
                var genericType = type.GetGenericTypeDefinition();
                if (genericType == typeof(IEnumerable<>) ||
                    genericType == typeof(ICollection<>) ||
                    genericType == typeof(IList<>) ||
                    genericType == typeof(List<>))
                {
                    instanceType = type.GetGenericArguments()[0];
                    var castMethod = typeof(Enumerable).GetMethod("Cast").MakeGenericMethod(instanceType);
                    var toListMethod = typeof(Enumerable).GetMethod("ToList").MakeGenericMethod(instanceType);
                    return instances => (TService)toListMethod.Invoke(null, new[] { castMethod.Invoke(null, new object[] { instances }) });
                }
            }

            if (type.IsArray)
            {
                instanceType = type.GetElementType();
                var castMethod = typeof(Enumerable).GetMethod("Cast").MakeGenericMethod(instanceType);
                var toArrayMethod = typeof(Enumerable).GetMethod("ToArray").MakeGenericMethod(instanceType);
                return instances => (TService)toArrayMethod.Invoke(null, new[] { castMethod.Invoke(null, new object[] { instances }) });
            }

            instanceType = null;
            return null;
        }
    }
}