using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Services.SampleImport.TemporaryDataAccessors;

namespace WB.Core.SharedKernels.DataCollection.Tests.ServiceTests
{
    public class InMemoryTemporaryDataRepositoryAccessor : ITemporaryDataRepositoryAccessor
    {
        private readonly Dictionary<string,object> container=new Dictionary<string, object>(); 
        public void Store<T>(T payload, string name) where T : class
        {
            container[name] = payload;
        }

        public T GetByName<T>(string name) where T : class
        {
            if (!container.ContainsKey(name))
                return null;
            return container[name] as T;
        }
    }
}
