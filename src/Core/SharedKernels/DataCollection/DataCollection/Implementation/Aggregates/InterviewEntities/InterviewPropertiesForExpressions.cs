using WB.Core.SharedKernels.DataCollection.ExpressionStorage;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public class InterviewPropertiesForExpressions : IInterviewPropertiesForExpressions
    {
        public double Random { get; set; }

        public InterviewPropertiesForExpressions(DataCollection.InterviewProperties interviewProperties)
        {
            this.Random = interviewProperties.IRnd();
        }
    }
}
