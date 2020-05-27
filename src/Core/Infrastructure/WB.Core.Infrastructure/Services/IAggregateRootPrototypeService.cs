﻿using System;
using System.Diagnostics;
using Microsoft.Extensions.Caching.Memory;

namespace WB.Core.Infrastructure.Services
{
    public interface IAggregateRootPrototypeService
    {
        PrototypeType? GetPrototypeType(Guid id);
        void MarkAsPrototype(Guid id, PrototypeType type);
        void RemovePrototype(Guid id);
    }
    
    public enum PrototypeType { Permanent, Temporary }
}
