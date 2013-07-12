using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.View.Question;


namespace Core.Supervisor.Views.TakeNew
{
    public class TakeNewInterviewView
    {
        public TakeNewInterviewView(IQuestionnaireDocument questionnaire)
        {
            this.QuestionnaireTitle = questionnaire.Title;
            this.QuestionnaireId = questionnaire.PublicKey;
            this.FeaturedQuestions = new List<QuestionView>();
            this.Supervisors = new List<UserDocument>();
            foreach (IQuestion q in questionnaire.GetFeaturedQuestions())
            {
                var questionView = new QuestionView(questionnaire, q);
                this.FeaturedQuestions.Add(questionView);
            }
        }

        public List<QuestionView> FeaturedQuestions { get; set; }

        public string QuestionnaireTitle { get; set; }

        public Guid QuestionnaireId { get; set; }

        public List<UserDocument> Supervisors { get; set; }
    }
}