// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RegisterGenericTypesOfInterface.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The register generic types of interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Conventions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Ninject.Activation;
    using Ninject.Extensions.Conventions.BindingGenerators;
    using Ninject.Syntax;

    /// <summary>
    /// The register generic types of interface.
    /// </summary>
    public class RegisterGenericTypesOfInterface : IBindingGenerator
    {
        #region Fields

        /// <summary>
        /// The base interface.
        /// </summary>
        private readonly Type baseInterface;

        /// <summary>
        /// The scope.
        /// </summary>
        private Func<IContext, object> scope;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterGenericTypesOfInterface"/> class.
        /// </summary>
        /// <param name="baseInterface">
        /// The base interface.
        /// </param>
        public RegisterGenericTypesOfInterface(Type baseInterface)
        {
            this.baseInterface = baseInterface;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterGenericTypesOfInterface"/> class.
        /// </summary>
        /// <param name="baseInterface">
        /// The base interface.
        /// </param>
        /// <param name="scope">
        /// The scope.
        /// </param>
        public RegisterGenericTypesOfInterface(Type baseInterface, Func<IContext, object> scope)
        {
            this.baseInterface = baseInterface;
            this.scope = scope;
        }

        #endregion

        /*    public void Process(Type type, Func<IContext, object> scopeCallback, IKernel kernel)
        {
            if (type.IsAbstract) { return; }
            if (type.IsInterface) { return; }
            var originalInterface = type.GetInterfaces().FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == baseInterface);
            if (originalInterface == null) return;

            Type[] wrappedTypes = originalInterface.GetGenericArguments();

            // Create the created type
            Type implementationType = baseInterface.MakeGenericType(wrappedTypes);

            // And specify what we're going to use
            kernel.Bind(implementationType).To(type);
        }*/
        #region Public Methods and Operators

        /// <summary>
        /// The create bindings.
        /// </summary>
        /// <param name="targetType">
        /// The target type.
        /// </param>
        /// <param name="bindingRoot">
        /// The binding root.
        /// </param>
        /// <returns>
        /// The System.Collections.Generic.IEnumerable`1[T -&gt; Ninject.Syntax.IBindingWhenInNamedWithOrOnSyntax`1[T -&gt; System.Object]].
        /// </returns>
        public IEnumerable<IBindingWhenInNamedWithOrOnSyntax<object>> CreateBindings(
            Type targetType, IBindingRoot bindingRoot)
        {
            // Assembly containingAssembly = type.Assembly;
            if (!this.baseInterface.IsGenericType)
            {
                return Enumerable.Empty<IBindingWhenInNamedWithOrOnSyntax<object>>();
            }

            IEnumerable<Type> matchedTypes =
                targetType.GetInterfaces().Where(
                    i => i.IsGenericType && i.GetGenericTypeDefinition() == this.baseInterface);

            // var result = new List<IBindingWhenInNamedWithOrOnSyntax<object>>();
            var retval = new List<IBindingWhenInNamedWithOrOnSyntax<object>>();
            foreach (Type matchedType in matchedTypes)
            {
                Type[] wrappedTypes = matchedType.GetGenericArguments();

                // Create the created type
                Type genericInterface = this.baseInterface.MakeGenericType(wrappedTypes);
                retval.Add(bindingRoot.Bind(genericInterface).To(targetType));

                /*  var factoryInterface = type.MakeGenericType(originalInterface);
              //  var factory = typeof(RepositoryFactory<,>).MakeGenericType(type, originalInterface);

                // Bind the factory interface to the implementation
                result.Add(bindingRoot.Bind(factoryInterface).To(matchedGenericType));*/
            }

            return retval;
        }

        #endregion
    }
}