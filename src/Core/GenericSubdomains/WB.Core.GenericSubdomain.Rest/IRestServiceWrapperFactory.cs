using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WB.Core.GenericSubdomain.Rest
{
    public interface IRestServiceWrapperFactory
    {
        IRestServiceWrapper CreateRestServiceWrapper(string baseAddress);
    }
}
