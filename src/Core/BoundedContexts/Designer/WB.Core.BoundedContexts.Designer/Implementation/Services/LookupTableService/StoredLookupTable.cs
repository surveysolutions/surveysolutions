using System;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService
{
    [StoredIn(typeof(StoredLookupTable))]
    public class StoredLookupTable : KeyValueEntity
    {
        
    }

    public class StoredInAttribute : Attribute
    {
        public Type StoredIn { get; }

        public StoredInAttribute(Type storedIn)
        {
            StoredIn = storedIn;
        }
    }
}
