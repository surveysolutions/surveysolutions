using System;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;

public class StoredInAttribute : Attribute
{
    public Type StoredIn { get; }

    public StoredInAttribute(Type storedIn)
    {
        StoredIn = storedIn;
    }
}
