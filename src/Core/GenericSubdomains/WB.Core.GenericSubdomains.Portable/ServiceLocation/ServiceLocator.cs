using System;

namespace WB.Core.GenericSubdomains.Portable.ServiceLocation
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
}