using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public class InterviewVariables : IReadSideRepositoryEntity
    {
        public InterviewVariables()
        {
            this.DisabledVariables = new HashSet<InterviewItemId>();
            this.VariableValues=new Dictionary<InterviewItemId, object>();
        }

        public InterviewVariables(Dictionary<InterviewItemId, object> variableValues, HashSet<InterviewItemId> disabledVariables)
        {
            this.VariableValues = variableValues;
            this.DisabledVariables = disabledVariables;
        }

        public Dictionary<InterviewItemId, object> VariableValues { get; private set; }
        public HashSet<InterviewItemId> DisabledVariables { get; private set; }
    }
}