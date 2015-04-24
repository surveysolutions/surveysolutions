using System;
using Main.Core.Entities.SubEntities;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public class QuestionAnswer
    {
        public virtual int Id { get; set; }

        public virtual Guid Questionid { get; set; }
        public virtual string Title { get; set; }
        public virtual string Answer { get; set; }
        public virtual QuestionType Type { get; set; }
        public virtual InterviewSummary InterviewSummary { get; set; }
    }
}