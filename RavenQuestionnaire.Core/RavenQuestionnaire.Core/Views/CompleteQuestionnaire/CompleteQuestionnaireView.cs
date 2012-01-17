using System;
using System.Linq;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Iterators;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.Group;
using RavenQuestionnaire.Core.Views.Question;
using RavenQuestionnaire.Core.Views.Questionnaire;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire
{
    public class CompleteQuestionnaireView:QuestionnaireView
            <CompleteGroup, CompleteQuestion,
            CompleteAnswer>
    {
        public SurveyStatus Status { get; set; }

        public UserLight Responsible { set; get; }
         public CompleteQuestionnaireView()
        {
        }
        public CompleteQuestionnaireView(IQuestionnaireDocument doc)
            : base(doc)
        {
        }

        public CompleteQuestionnaireView(CompleteQuestionnaireDocument doc)
            : base(doc)
        {
            this.Status = doc.Status;
            this.Responsible = doc.Responsible;
            this.Questions = doc.Questions.Select(q => new CompleteQuestionView(doc, q)).ToArray();
        }
    }
}
