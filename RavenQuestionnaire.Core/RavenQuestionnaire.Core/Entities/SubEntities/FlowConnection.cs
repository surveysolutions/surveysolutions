using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.SubEntities
{
    public class FlowConnection
    {
        public FlowConnection()
        {
        }

        public FlowConnection(Guid sourceId, Guid targetId)
        {
            Source = sourceId;
            Target = targetId;
        }

        public Guid Source { get; set; }
        public Guid Target { get; set; }
        public string LabelText { get; set; }
        public string Condition { get; set; }
    }
}
