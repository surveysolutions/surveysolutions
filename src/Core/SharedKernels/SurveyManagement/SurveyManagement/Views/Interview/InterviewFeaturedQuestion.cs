using System;
using Main.Core.Entities.SubEntities;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public class InterviewFeaturedQuestion
    {
        public Guid Id { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public QuestionType Type { get; set; }
    }
}
