using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.View.Question;

namespace Core.Supervisor.Views.Assign
{
    public class AssignSurveyView
    {
        public AssignSurveyView(ICompleteQuestionnaireDocument completeQuestionnaire)
        {
            this.Id = completeQuestionnaire.PublicKey;
            this.QuestionnaireTitle = completeQuestionnaire.Title;
            this.TemplateId = completeQuestionnaire.TemplateId;
            this.Status = completeQuestionnaire.Status;
            this.Responsible = completeQuestionnaire.Responsible;
            this.FeaturedQuestions = new List<CompleteQuestionView>();
            this.Supervisors = new List<UserDocument>();

            foreach (ICompleteQuestion q in completeQuestionnaire.GetFeaturedQuestions())
            {
                var questionView = new CompleteQuestionView(completeQuestionnaire, q);
                this.FeaturedQuestions.Add(questionView);
            }
        }

        public List<CompleteQuestionView> FeaturedQuestions { get; set; }

        public Guid Id { get; set; }

        public string QuestionnaireTitle { get; set; }

        public UserLight Responsible { get; set; }

        public SurveyStatus Status { get; set; }

        public Guid TemplateId { get; set; }

        public List<UserDocument> Supervisors { get; set; }
    }
}