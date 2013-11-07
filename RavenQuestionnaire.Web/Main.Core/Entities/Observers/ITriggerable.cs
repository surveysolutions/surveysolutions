using System;
using System.Collections.Generic;

namespace Main.Core.Entities.Observers
{
    public interface ITriggerable
    {
        List<Guid> Triggers { get; set; }
    }
}