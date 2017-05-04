using System;
using NConsole;
using Ninject;

namespace support
{
    public class ConsoleDependencyResolver : IDependencyResolver
    {
        private readonly IKernel _ninjectKernel;

        public ConsoleDependencyResolver(IKernel ninjectKernel)
        {
            _ninjectKernel = ninjectKernel;
        }

        public object GetService(Type serviceType) => _ninjectKernel.Get(serviceType);
    }
}