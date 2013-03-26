//-------------------------------------------------------------------------------
// <copyright file="BindToExtensions.cs" company="Ninject Project Contributors">
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
    using System;
    using System.Linq.Expressions;

    using Castle.DynamicProxy;
    using Ninject.Activation;
    using Ninject.Syntax;

    /// <summary>
    /// Extension methods for <see cref="IBindingToSyntax{TInterface}"/>
    /// </summary>
    public static class BindToExtensions
    {
        /// <summary>
        /// Defines that the interface shall be bound to an automatically created factory proxy.
        /// </summary>
        /// <typeparam name="TInterface">The type of the interface.</typeparam>
        /// <param name="syntax">The syntax.</param>
        /// <returns>The <see cref="IBindingWhenInNamedWithOrOnSyntax{TInterface}"/> to configure more things for the binding.</returns>
        public static IBindingWhenInNamedWithOrOnSyntax<TInterface> ToFactory<TInterface>(this IBindingToSyntax<TInterface> syntax)
            where TInterface : class
        {
            return ToFactory(syntax, ctx => ctx.Kernel.Get<IInstanceProvider>(), typeof(TInterface));
        }

        /// <summary>
        /// Defines that the interface shall be bound to an automatically created factory proxy.
        /// </summary>
        /// <param name="syntax">The syntax.</param>
        /// <param name="factoryType">The type of the factory.</param>
        /// <returns>
        /// The <see cref="IBindingWhenInNamedWithOrOnSyntax{TInterface}"/> to configure more things for the binding.
        /// </returns>
        public static IBindingWhenInNamedWithOrOnSyntax<object> ToFactory(this IBindingToSyntax<object> syntax, Type factoryType)
        {
            return ToFactory(syntax, ctx => ctx.Kernel.Get<IInstanceProvider>(), factoryType);
        }

        /// <summary>
        /// Defines that the interface shall be bound to an automatically created factory proxy.
        /// </summary>
        /// <typeparam name="TInterface">The type of the interface.</typeparam>
        /// <param name="syntax">The syntax.</param>
        /// <param name="instanceProvider">The instance provider.</param>
        /// <returns>
        /// The <see cref="IBindingWhenInNamedWithOrOnSyntax{TInterface}"/> to configure more things for the binding.
        /// </returns>
        public static IBindingWhenInNamedWithOrOnSyntax<TInterface> ToFactory<TInterface>(this IBindingToSyntax<TInterface> syntax, Func<IInstanceProvider> instanceProvider)
            where TInterface : class
        {
            return ToFactory(syntax, ctx => instanceProvider(), typeof(TInterface));
        }

        /// <summary>
        /// Defines that the interface shall be bound to an automatically created factory proxy.
        /// </summary>
        /// <param name="syntax">The syntax.</param>
        /// <param name="instanceProvider">The instance provider.</param>
        /// <param name="factoryType">Type of the factory.</param>
        /// <returns>
        /// The <see cref="IBindingWhenInNamedWithOrOnSyntax{TInterface}"/> to configure more things for the binding.
        /// </returns>
        public static IBindingWhenInNamedWithOrOnSyntax<object> ToFactory(this IBindingToSyntax<object> syntax, Func<IInstanceProvider> instanceProvider, Type factoryType)
        {
            return ToFactory(syntax, ctx => instanceProvider(), factoryType);
        }

        /// <summary>
        /// Defines a named binding with the name taken from the factory method used to create instances.
        /// </summary>
        /// <typeparam name="TInterface">The type of the interface.</typeparam>
        /// <typeparam name="TFactory">¨The type of the factory.</typeparam>
        /// <param name="syntax">The syntax.</param>
        /// <param name="action">Expression defining the factory method used to get the binding name from.</param>
        /// <returns>
        /// The <see cref="IBindingWithOrOnSyntax{TInterface}"/> to configure more things for the binding.
        /// </returns>
        public static IBindingWithOrOnSyntax<TInterface> NamedLikeFactoryMethod<TInterface, TFactory>(this IBindingNamedSyntax<TInterface> syntax, Expression<Action<TFactory>> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            var methodCallExpression = action.Body as MethodCallExpression;

            if (methodCallExpression == null)
            {
                throw new ArgumentException("expected factory method instead of " + action, "action");
            }

            var methodName = methodCallExpression.Method.Name;

            if (!methodName.StartsWith("Get"))
            {
                throw new ArgumentException("expected factory 'Get' method instead of " + action, "action");
            }

            var bindingName = methodName.Substring(3);

            return syntax.Named(bindingName);
        }

        /// <summary>
        /// Defines that the interface shall be bound to an automatically created factory proxy.
        /// </summary>
        /// <typeparam name="TInterface">The type of the interface.</typeparam>
        /// <param name="syntax">The syntax.</param>
        /// <param name="instanceProvider">The instance provider.</param>
        /// <param name="factoryType">Type of the factory.</param>
        /// <returns>
        /// The <see cref="IBindingWhenInNamedWithOrOnSyntax{TInterface}"/> to configure more things for the binding.
        /// </returns>
        private static IBindingWhenInNamedWithOrOnSyntax<TInterface> ToFactory<TInterface>(IBindingToSyntax<TInterface> syntax, Func<IContext, IInstanceProvider> instanceProvider, Type factoryType) 
            where TInterface : class
        {
            var proxy = new ProxyGenerator().ProxyBuilder.CreateInterfaceProxyTypeWithoutTarget(
                factoryType, new[] { typeof(IFactoryProxy) }, ProxyGenerationOptions.Default);
            var result = syntax.To(proxy);
            result.WithParameter(new ProxyTargetParameter());

            var bindingConfiguration = syntax.BindingConfiguration; // Do not pass syntax to the lambda!!! We do not want the lambda referencing the syntax!!!
            syntax.Kernel.Bind<IInstanceProvider>().ToMethod(instanceProvider)
                .When(r => r.ParentRequest != null && r.ParentRequest.ParentContext.Binding.BindingConfiguration == bindingConfiguration)
                .InScope(ctx => bindingConfiguration);

            return result;
        }
    }
}
#endif