using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Headquarters.Views.TakeNew
{
    public class TakeNewAssignmentView
    {
        public TakeNewAssignmentView(IQuestionnaireDocument questionnaire)
        {
            this.QuestionnaireTitle = questionnaire.Title;
            this.QuestionnaireId = questionnaire.PublicKey;
            this.FeaturedQuestions = new List<FeaturedQuestionView>();

            foreach (IQuestion q in questionnaire.Find<IQuestion>(x => x.Featured))
            {
                var questionView = new FeaturedQuestionView(q, null);
                this.FeaturedQuestions.Add(questionView);
            }
        }

        public List<FeaturedQuestionView> FeaturedQuestions { get; set; }

        public string QuestionnaireTitle { get; set; }

        public Guid QuestionnaireId { get; set; }

        public long QuestionnaireVersion { get; set; }
    }
}
