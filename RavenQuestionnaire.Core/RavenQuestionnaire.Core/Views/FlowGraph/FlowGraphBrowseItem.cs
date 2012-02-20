using System;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.FlowGraph
{
    public class FlowGraphBrowseItem
    {
        public string Id { 
            get { return IdUtil.ParseId(_id); } 
            private set { _id = value; }
        }

        private string _id;

        public DateTime CreationDate { get; private set; }
        public DateTime LastEntryDate { get; private set; }

        public FlowGraphBrowseItem(string id,DateTime creationDate, DateTime lastEntryDate)
        {
            this.Id = id;
            this.CreationDate = creationDate;
            this.LastEntryDate = lastEntryDate;
        }
        public static FlowGraphBrowseItem New()
        {
            return new FlowGraphBrowseItem(null, DateTime.Now, DateTime.Now);
        }
    }
}
