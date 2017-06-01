using System;
using WB.Core.SharedKernels.DataCollection.ExpressionStorage;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public class InterviewPropertiesForExpressions : IInterviewPropertiesForExpressions
    {
        private readonly InterviewProperties properties;
        public double Random { get; }

        public Guid SupervisorId => this.properties.SupervisorId ?? Guid.Empty;

        public Guid InterviewerId => this.properties.InterviewerId ?? Guid.Empty;

        public string InterviewId => this.properties.Id;

        public InterviewPropertiesForExpressions(
            DataCollection.InterviewProperties interviewProperties, 
            InterviewProperties properties)
        {
            this.properties = properties;
            this.Random = interviewProperties.IRnd();
        }
    }
}
