using System.Collections.Generic;
using WB.Core.Infrastructure;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests
{
    public class InMemoryTemporaryDataRepositoryAccessor<T> : ITemporaryDataStorage<T> where T : class
    {
        private readonly Dictionary<string,T> container=new Dictionary<string, T>(); 

        public void Store(T payload, string name)
        {
            container[name] = payload;
        }

        public T GetByName(string name)
        {
            if (!container.ContainsKey(name))
                return null;
            return container[name];
        }
    }
}
