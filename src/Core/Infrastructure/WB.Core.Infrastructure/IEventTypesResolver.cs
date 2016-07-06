
using System;

namespace WB.Core.Infrastructure
{
    public interface IEventTypesResolver
    {
        Type GetTypeByName(string implementationTypeName);
    }
}
