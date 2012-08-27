using System;
using System.Collections.Generic;
using System.Linq;
using RavenQuestionnaire.Core.AbstractFactories;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using RavenQuestionnaire.Core.Views.Question;
using RavenQuestionnaire.Core.Views.User;

namespace RavenQuestionnaire.Core.Views.Assign
{
    public class AssignSurveyView
    {
        public Guid Id { get; set; }
        public string QuestionnaireTitle { get; set; }
        public string TemplateId { get; set; }
        public SurveyStatus Status { get; set; }
        public UserLight Responsible { get; set; }
        public List<CompleteQuestionView> FeaturedQuestions { get; set; }

        public AssignSurveyView(CompleteQuestionnaireBrowseItem doc, CompleteQuestionnaireStoreDocument completeQuestionnaire)
        {
            Id = completeQuestionnaire.PublicKey;
            QuestionnaireTitle = doc.QuestionnaireTitle;
            TemplateId = doc.TemplateId;
            Status = doc.Status;
            Responsible = doc.Responsible;
            FeaturedQuestions = new List<CompleteQuestionView>();
            foreach (var q in doc.FeaturedQuestions)
            {
                var question = completeQuestionnaire.Find<ICompleteQuestion>(q.PublicKey);
                var questionView = new CompleteQuestionFactory().CreateQuestion(completeQuestionnaire, question);
                questionView.ParentGroupPublicKey = q.GroupPublicKey;
                FeaturedQuestions.Add(questionView);
            }
        }
    }
}
