using System;

namespace WB.Core.Infrastructure.Services
{
    public static class AggregatePrototypeExtensions
    {
        public static bool IsPrototype(this IAggregateRootPrototypeService service, Guid id)
        {
            return service.GetPrototypeType(id) != null;
        }
    }
}
