using System;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Documents.Statistics
{
    public class CompleteQuestionnaireStatisticDocument
    {
        public CompleteQuestionnaireStatisticDocument()
        {
            this.AnsweredQuestions = new List<QuestionStatisticDocument>();
            this.InvalidQuestions = new List<QuestionStatisticDocument>();
            this.FeturedQuestions = new List<QuestionStatisticDocument>();
        }

        public string Id { get; set; }
        public string Title { get; set; }
        public string CompleteQuestionnaireId { get; set; }
        public string TemplateId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public IList<QuestionStatisticDocument> AnsweredQuestions { get; set; }
        public IList<QuestionStatisticDocument> InvalidQuestions { get; set; }
        public IList<QuestionStatisticDocument> FeturedQuestions { get; set; }
        public SurveyStatus Status { set; get; }
        public int TotalQuestionCount { get; set; }
    }
}
