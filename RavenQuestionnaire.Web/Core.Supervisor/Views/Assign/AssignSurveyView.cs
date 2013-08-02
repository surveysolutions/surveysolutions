using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.View.Question;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace Core.Supervisor.Views.Assign
{
    public class AssignSurveyView
    {
        public AssignSurveyView(QuestionnaireBrowseItem completeQuestionnaire, Guid questionnarieId)
        {
            this.Id = questionnarieId;
            this.QuestionnaireTitle = completeQuestionnaire.Title;
            this.TemplateId = completeQuestionnaire.QuestionnaireId;
            this.Status = SurveyStatus.Unknown;
            this.Responsible = null;
            this.FeaturedQuestions = new List<CompleteQuestionView>();
            this.Supervisors = new List<UserDocument>();

            foreach (var q in completeQuestionnaire.FeaturedQuestions)
            {
                var questionView = new CompleteQuestionView(questionnarieId.ToString(), null)
                    {
                        Title = q.Title,
                        PublicKey = q.Id,
                        StataExportCaption = q.Caption
                    };
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