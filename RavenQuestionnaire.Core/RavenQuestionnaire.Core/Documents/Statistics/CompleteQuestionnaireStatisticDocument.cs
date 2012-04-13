using System;
using System.Collections.Generic;

namespace RavenQuestionnaire.Core.Documents.Statistics
{
    public class CompleteQuestionnaireStatisticDocument
    {
        public CompleteQuestionnaireStatisticDocument()
        {
            this.AnsweredQuestions = new List<QuestionStatisticDocument>();
            this.InvalidQuestions = new List<QuestionStatisticDocument>();
        }

        public string Id { get; set; }
        public string Title { get; set; }
        public string CompleteQuestionnaireId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public IList<QuestionStatisticDocument> AnsweredQuestions { get; set; }
        public IList<QuestionStatisticDocument> InvalidQuestions { get; set; }
    }
}
