using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public class InterviewLinkedQuestionOptions : IReadSideRepositoryEntity
    {
        public InterviewLinkedQuestionOptions()
        {
            this.LinkedQuestionOptions = new Dictionary<string, RosterVector[]>();
        }

        public InterviewLinkedQuestionOptions(Dictionary<string, RosterVector[]> linkedQuestionOptions)
        {
            this.LinkedQuestionOptions = linkedQuestionOptions;
        }

        public Dictionary<string, RosterVector[]> LinkedQuestionOptions { get; private set; }
    }
}