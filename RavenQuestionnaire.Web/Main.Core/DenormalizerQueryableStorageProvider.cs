using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Main.DenormalizerStorage;
using Ninject;
using Ninject.Activation;

namespace Main.Core
{
    public class DenormalizerQueryableStorageProvider<T> : Provider<IQueryableDenormalizerStorage<T>>
        where T : class
    {
        protected override IQueryableDenormalizerStorage<T> CreateInstance(IContext context)
        {
            return context.Kernel.Get<InMemoryDenormalizer<T>>();
        }

    }
}
