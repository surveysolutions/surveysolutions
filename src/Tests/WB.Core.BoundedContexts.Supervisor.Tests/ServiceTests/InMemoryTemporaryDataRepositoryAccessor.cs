using System.Collections.Generic;
using WB.Core.BoundedContexts.Supervisor.Services;

namespace WB.Core.BoundedContexts.Supervisor.Tests.ServiceTests
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
