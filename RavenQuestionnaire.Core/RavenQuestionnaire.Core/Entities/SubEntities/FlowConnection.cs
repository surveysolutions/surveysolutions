using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.SubEntities
{
    public class FlowConnection 
    {
        private Guid sourceId;
        private Guid targetId;

        public FlowConnection()
        {
        }

        public FlowConnection(Guid sourceId, Guid targetId)
        {
            // TODO: Complete member initialization
            this.sourceId = sourceId;
            this.targetId = targetId;
        }

        public string Source { get; set; }
        public string Target { get; set; }
        public string LabelText { get; set; }
    }
}
