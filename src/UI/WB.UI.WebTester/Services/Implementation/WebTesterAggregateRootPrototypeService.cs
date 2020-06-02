using System;
using WB.Core.Infrastructure.Services;

namespace WB.UI.WebTester.Services.Implementation
{
    public class WebTesterAggregateRootPrototypeService : IAggregateRootPrototypeService
    {
        public PrototypeType? GetPrototypeType(Guid id)
        {
            return PrototypeType.Permanent;
        }

        public void MarkAsPrototype(Guid id, PrototypeType type)
        {
            
        }

        public void RemovePrototype(Guid id)
        {

        }
    }
}
