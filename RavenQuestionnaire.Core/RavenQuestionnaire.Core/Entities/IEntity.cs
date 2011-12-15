using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RavenQuestionnaire.Core.Entities
{
    public interface IEntity<TDoc>
    {
        TDoc GetInnerDocument();
    }
}
