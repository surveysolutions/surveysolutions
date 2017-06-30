using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Practices.ServiceLocation
{
    /// <summary>
    ///     This class provides the ambient container for this application. If your
    ///     framework defines such an ambient container, use ServiceLocator.Current
    ///     to get it.
    /// </summary>
    public static class ServiceLocator
    {
        private static ServiceLocatorProvider currentProvider;

        /// <summary>
        ///     The current ambient container.
        /// </summary>
        public static IServiceLocator Current
        {
            get
            {
                if (!IsLocationProviderSet) throw new InvalidOperationException("Service location not set");

                return currentProvider();
            }
        }

        public static bool IsLocationProviderSet => currentProvider != null;

        /// <summary>
        ///     Set the delegate that is used to retrieve the current container.
        /// </summary>
        /// <param name="newProvider">
        ///     Delegate that, when called, will return
        ///     the current ambient container.
        /// </param>
        public static void SetLocatorProvider(ServiceLocatorProvider newProvider)
        {
            currentProvider = newProvider;
        }
    }

    /// <summary>
    ///     This delegate type is used to provide a method that will
    ///     return the current container. Used with the <see cref="ServiceLocator" />
    ///     static accessor class.
    /// </summary>
    /// <returns>An <see cref="IServiceLocator" />.</returns>
    public delegate IServiceLocator ServiceLocatorProvider();


    /// <summary>
    ///     The generic Service Locator interface. This interface is used
    ///     to retrieve services (instances identified by type and optional
    ///     name) from a container.
    /// </summary>
    public interface IServiceLocator : IServiceProvider
    {
        /// <summary>
        ///     Get an instance of the given <paramref name="serviceType" />.
        /// </summary>
        /// <param name="serviceType">Type of object requested.</param>
        /// <exception cref="ActivationException">
        ///     if there is an error resolving
        ///     the service instance.
        /// </exception>
        /// <returns>The requested service instance.</returns>
        object GetInstance(Type serviceType);

        /// <summary>
        ///     Get an instance of the given named <paramref name="serviceType" />.
        /// </summary>
        /// <param name="serviceType">Type of object requested.</param>
        /// <param name="key">Name the object was registered with.</param>
        /// <exception cref="ActivationException">
        ///     if there is an error resolving
        ///     the service instance.
        /// </exception>
        /// <returns>The requested service instance.</returns>
        object GetInstance(Type serviceType, string key);

        /// <summary>
        ///     Get all instances of the given <paramref name="serviceType" /> currently
        ///     registered in the container.
        /// </summary>
        /// <param name="serviceType">Type of object requested.</param>
        /// <exception cref="ActivationException">
        ///     if there is are errors resolving
        ///     the service instance.
        /// </exception>
        /// <returns>A sequence of instances of the requested <paramref name="serviceType" />.</returns>
        IEnumerable<object> GetAllInstances(Type serviceType);

        /// <summary>
        ///     Get an instance of the given <typeparamref name="TService" />.
        /// </summary>
        /// <typeparam name="TService">Type of object requested.</typeparam>
        /// <exception cref="ActivationException">
        ///     if there is are errors resolving
        ///     the service instance.
        /// </exception>
        /// <returns>The requested service instance.</returns>
        TService GetInstance<TService>();

        /// <summary>
        ///     Get an instance of the given named <typeparamref name="TService" />.
        /// </summary>
        /// <typeparam name="TService">Type of object requested.</typeparam>
        /// <param name="key">Name the object was registered with.</param>
        /// <exception cref="ActivationException">
        ///     if there is are errors resolving
        ///     the service instance.
        /// </exception>
        /// <returns>The requested service instance.</returns>
        TService GetInstance<TService>(string key);

        /// <summary>
        ///     Get all instances of the given <typeparamref name="TService" /> currently
        ///     registered in the container.
        /// </summary>
        /// <typeparam name="TService">Type of object requested.</typeparam>
        /// <exception cref="ActivationException">
        ///     if there is are errors resolving
        ///     the service instance.
        /// </exception>
        /// <returns>A sequence of instances of the requested <typeparamref name="TService" />.</returns>
        IEnumerable<TService> GetAllInstances<TService>();
    }

    /// <summary>
    /// This class is a helper that provides a default implementation
    /// for most of the methods of <see cref="IServiceLocator"/>.
    /// </summary>
    public abstract class ServiceLocatorImplBase : IServiceLocator
    {
        /// <summary>
        /// Implementation of <see cref="IServiceProvider.GetService"/>.
        /// </summary>
        /// <param name="serviceType">The requested service.</param>
        /// <exception cref="ActivationException">if there is an error in resolving the service instance.</exception>
        /// <returns>The requested object.</returns>
        public virtual object GetService(Type serviceType)
        {
            return GetInstance(serviceType, null);
        }

        /// <summary>
        /// Get an instance of the given <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="serviceType">Type of object requested.</param>
        /// <exception cref="ActivationException">if there is an error resolving
        /// the service instance.</exception>
        /// <returns>The requested service instance.</returns>
        public virtual object GetInstance(Type serviceType)
        {
            return GetInstance(serviceType, null);
        }

        /// <summary>
        /// Get an instance of the given named <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="serviceType">Type of object requested.</param>
        /// <param name="key">Name the object was registered with.</param>
        /// <exception cref="ActivationException">if there is an error resolving
        /// the service instance.</exception>
        /// <returns>The requested service instance.</returns>
        public virtual object GetInstance(Type serviceType, string key)
        {
            try
            {
                return DoGetInstance(serviceType, key);
            }
            catch (Exception ex)
            {
                throw new ActivationException(
                    FormatActivationExceptionMessage(ex, serviceType, key),
                    ex);
            }
        }

        /// <summary>
        /// Get all instances of the given <paramref name="serviceType"/> currently
        /// registered in the container.
        /// </summary>
        /// <param name="serviceType">Type of object requested.</param>
        /// <exception cref="ActivationException">if there is are errors resolving
        /// the service instance.</exception>
        /// <returns>A sequence of instances of the requested <paramref name="serviceType"/>.</returns>
        public virtual IEnumerable<object> GetAllInstances(Type serviceType)
        {
            try
            {
                return DoGetAllInstances(serviceType);
            }
            catch (Exception ex)
            {
                throw new ActivationException(
                    FormatActivateAllExceptionMessage(ex, serviceType),
                    ex);
            }
        }

        /// <summary>
        /// Get an instance of the given <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">Type of object requested.</typeparam>
        /// <exception cref="ActivationException">if there is are errors resolving
        /// the service instance.</exception>
        /// <returns>The requested service instance.</returns>
        public virtual TService GetInstance<TService>()
        {
            return (TService)GetInstance(typeof(TService), null);
        }

        /// <summary>
        /// Get an instance of the given named <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">Type of object requested.</typeparam>
        /// <param name="key">Name the object was registered with.</param>
        /// <exception cref="ActivationException">if there is are errors resolving
        /// the service instance.</exception>
        /// <returns>The requested service instance.</returns>
        public virtual TService GetInstance<TService>(string key)
        {
            return (TService)GetInstance(typeof(TService), key);
        }

        /// <summary>
        /// Get all instances of the given <typeparamref name="TService"/> currently
        /// registered in the container.
        /// </summary>
        /// <typeparam name="TService">Type of object requested.</typeparam>
        /// <exception cref="ActivationException">if there is are errors resolving
        /// the service instance.</exception>
        /// <returns>A sequence of instances of the requested <typeparamref name="TService"/>.</returns>
        public virtual IEnumerable<TService> GetAllInstances<TService>()
        {
            foreach (object item in GetAllInstances(typeof(TService)))
            {
                yield return (TService)item;
            }
        }

        /// <summary>
        /// When implemented by inheriting classes, this method will do the actual work of resolving
        /// the requested service instance.
        /// </summary>
        /// <param name="serviceType">Type of instance requested.</param>
        /// <param name="key">Name of registered service you want. May be null.</param>
        /// <returns>The requested service instance.</returns>
        protected abstract object DoGetInstance(Type serviceType, string key);

        /// <summary>
        /// When implemented by inheriting classes, this method will do the actual work of
        /// resolving all the requested service instances.
        /// </summary>
        /// <param name="serviceType">Type of service requested.</param>
        /// <returns>Sequence of service instance objects.</returns>
        protected abstract IEnumerable<object> DoGetAllInstances(Type serviceType);

        /// <summary>
        /// Format the exception message for use in an <see cref="ActivationException"/>
        /// that occurs while resolving a single service.
        /// </summary>
        /// <param name="actualException">The actual exception thrown by the implementation.</param>
        /// <param name="serviceType">Type of service requested.</param>
        /// <param name="key">Name requested.</param>
        /// <returns>The formatted exception message string.</returns>
        protected virtual string FormatActivationExceptionMessage(Exception actualException, Type serviceType, string key)
        {
            return string.Format(CultureInfo.CurrentUICulture, "Activation error occurred while trying to get instance of type {0}, key {1}", serviceType.Name, key);
        }

        /// <summary>
        /// Format the exception message for use in an <see cref="ActivationException"/>
        /// that occurs while resolving multiple service instances.
        /// </summary>
        /// <param name="actualException">The actual exception thrown by the implementation.</param>
        /// <param name="serviceType">Type of service requested.</param>
        /// <returns>The formatted exception message string.</returns>
        protected virtual string FormatActivateAllExceptionMessage(Exception actualException, Type serviceType)
        {
            return string.Format(CultureInfo.CurrentUICulture, "Activation error occurred while trying to get all instances of type {0}", serviceType.Name);
        }
    }

    /// <summary>
    /// The standard exception thrown when a ServiceLocator has an error in resolving an object.
    /// </summary>
    public partial class ActivationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Exception" /> class.
        /// </summary>
        public ActivationException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Exception" /> class with a specified error message.
        /// </summary>
        /// <param name="message">
        /// The message that describes the error. 
        ///  </param>
        public ActivationException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Exception" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">
        /// The error message that explains the reason for the exception. 
        /// </param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified. 
        /// </param>
        public ActivationException(string message, Exception innerException) : base(message, innerException) { }
    }
}