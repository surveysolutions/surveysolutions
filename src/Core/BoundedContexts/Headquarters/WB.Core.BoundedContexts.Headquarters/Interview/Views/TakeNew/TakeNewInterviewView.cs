using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Utils;

namespace WB.Core.BoundedContexts.Headquarters.Interview.Views.TakeNew
{
    public class TakeNewInterviewView
    {
        public TakeNewInterviewView(IQuestionnaireDocument questionnaire, long questionnaireVersion)
        {
            this.QuestionnaireTitle = questionnaire.Title;
            this.QuestionnaireId = questionnaire.PublicKey;
            this.QuestionnaireVersion = questionnaireVersion;
            this.FeaturedQuestions = new List<FeaturedQuestionView>();
            
            //this.Supervisors = new List<UserDocument>();

            foreach (IQuestion q in questionnaire.GetAllQuestions().Where(x => x.Featured).ToList())
            {
                var questionView = new FeaturedQuestionView(q, null);
                this.FeaturedQuestions.Add(questionView);
            }
        }

        public List<FeaturedQuestionView> FeaturedQuestions { get; set; }

        public string QuestionnaireTitle { get; set; }

        public Guid QuestionnaireId { get; set; }

        //public List<UserDocument> Supervisors { get; set; }

        public long QuestionnaireVersion { get; set; }
    }
}