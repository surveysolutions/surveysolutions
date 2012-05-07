using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Documents.Statistics;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.Statistics
{
    public class CompleteQuestionnaireStatisticView
    {
        public CompleteQuestionnaireStatisticView(CompleteQuestionnaireStatisticDocument doc)
        {
            this.Id = doc.Id;
            this.Title = doc.Title;
            this.StartDate = doc.StartDate;
            this.EndDate = doc.EndDate;
            this.AnsweredQuestions = doc.AnsweredQuestions.Select(q => new QuestionStatisticView(q)).ToList();
            this.InvalidQuestions = doc.InvalidQuestions.Select(q => new QuestionStatisticView(q)).ToList();
            this.CompleteQuestionnaireId = IdUtil.ParseId(doc.CompleteQuestionnaireId);
            Creator = doc.Creator;
            FeaturedQuestions = doc.FeturedQuestions.Select(q => new QuestionStatisticView(q)).ToList();
        }
        public string Id { get; set; }
        public string Title { get; set; }
        public string CompleteQuestionnaireId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public UserLight Creator { get; set; }
        public IList<QuestionStatisticView> AnsweredQuestions { get; set; }
        public IList<QuestionStatisticView> InvalidQuestions { get; set; }
        public IList<QuestionStatisticView> FeaturedQuestions { get; set; }
    }
}
