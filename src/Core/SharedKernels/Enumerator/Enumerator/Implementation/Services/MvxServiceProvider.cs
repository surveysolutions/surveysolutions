using System;
using MvvmCross;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class MvxServiceProvider: IServiceProvider
    {
        public object GetService(Type serviceType) => Mvx.IoCProvider.Resolve(serviceType);
    }
}
