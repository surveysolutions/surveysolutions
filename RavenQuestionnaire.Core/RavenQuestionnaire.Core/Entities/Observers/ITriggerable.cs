using System;
using System.Collections.Generic;

namespace RavenQuestionnaire.Core.Entities.Observers
{
    public interface ITriggerable
    {
        List<Guid> Triggers { get; set; }
    }
}
