using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser
{
    public class PreloadedInterviewData
    {
        public string ResponsibleName { get; set; }
        public List<InterviewAnswer> Answers { get; set; }
        public int? Quantity { get; set; }
    }

    
}