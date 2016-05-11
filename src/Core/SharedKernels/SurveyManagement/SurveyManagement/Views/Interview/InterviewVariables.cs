using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public class InterviewVariables : IReadSideRepositoryEntity
    {
        public InterviewVariables()
        {
            this.VariableValues=new Dictionary<string, object>();
        }

        public InterviewVariables(Dictionary<string, object> variableValues)
        {
            this.VariableValues = variableValues;
        }

        public Dictionary<string, object> VariableValues { get; private set; }
    }
}