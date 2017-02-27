using System.Collections.Generic;
using System.Diagnostics;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    [DebuggerDisplay("{Title} ({Id})")]
    public class InterviewGroupView : InterviewEntityView
    {
        public int Depth { set; get; }
        public string Title { get; set; }
        public InterviewGroupView(Identity id) 
        {
            this.Entities = new List<InterviewEntityView>();
            this.Id = id;
        }

        public InterviewGroupView() { }
        public List<InterviewEntityView> Entities { set; get; }
    }
}
