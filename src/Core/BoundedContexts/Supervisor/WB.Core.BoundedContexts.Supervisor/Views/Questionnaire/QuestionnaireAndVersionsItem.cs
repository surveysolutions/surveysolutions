using System;

namespace WB.Core.BoundedContexts.Supervisor.Views.Questionnaire
{
    public class QuestionnaireAndVersionsItem
    {
        public Guid QuestionnaireId { get; set; }
        public string Title { get; set; }
        public long[] Versions { get; set; }
    }
}