using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RavenQuestionnaire.Core.Entities.Observers
{
    public interface ITriggerable
    {
        List<Guid> Triggers { get; set; }
    }
}
