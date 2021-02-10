using System;
using System.Diagnostics;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    [DebuggerDisplay("{ToString()}")]
    public class InterviewReportAnswer : EntityWithTypedId<int>
    {
        public virtual InterviewSummary InterviewSummary { get; set; }
        
        private string value;
        public virtual string Value
        {
            get => value;
            set
            {
                this.value = value;
                this.ValueLowerCase = value?.ToLower();
            }
        }

        public virtual string ValueLowerCase { get; protected set; }
        public virtual QuestionnaireCompositeItem Entity { get; set; }

        public virtual decimal? AnswerCode { get; set; }
        public virtual bool IsEnabled { get; set; }
    }
}
