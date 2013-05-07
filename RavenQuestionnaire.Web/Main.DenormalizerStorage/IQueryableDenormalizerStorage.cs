using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Main.DenormalizerStorage
{
    public interface IQueryableDenormalizerStorage<T> : IDenormalizerStorage<T> where T : class
    {
        IQueryable<T> Query();
    }
}
