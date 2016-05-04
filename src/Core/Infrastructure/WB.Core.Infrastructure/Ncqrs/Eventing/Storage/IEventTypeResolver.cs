﻿using System;

namespace Ncqrs.Eventing.Storage
{
    public interface IEventTypeResolver
    {
        Type ResolveType(string eventName);

        void RegisterEventDataType(Type type);
    }
}
