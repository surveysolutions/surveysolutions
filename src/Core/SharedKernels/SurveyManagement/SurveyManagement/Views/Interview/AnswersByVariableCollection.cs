using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public class AnswersByVariableCollection : IReadSideRepositoryEntity
    {
        public AnswersByVariableCollection()
        {
            this.Answers = new Dictionary<Guid, Dictionary<string, string>>();
        }
        public Dictionary<Guid, Dictionary<string, string>> Answers { get; set; }
    }
}