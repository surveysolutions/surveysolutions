﻿using System;
using System.Collections.Generic;

namespace WB.Core.Infrastructure.Aggregates
{
    public interface IAggregateRootRepository
    {
        IAggregateRoot GetLatest(Type aggregateType, Guid aggregateId);
    }
}