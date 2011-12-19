using System;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Documents
{
    public class CompleteQuestionnaireDocument
    {
        public CompleteQuestionnaireDocument()
        {
            CreationDate = DateTime.Now;
            LastEntryDate = DateTime.Now;
            CompletedAnswers = new List<CompleteAnswer>();
        }

        public string Id { get; set; }

        public QuestionnaireDocument Questionnaire { get; set; }
        public string UserId { get; set; }

        public DateTime CreationDate { get; set; }
        public DateTime LastEntryDate { get; set; }

        public List<CompleteAnswer> CompletedAnswers { get; set; }

        public string Status { set; get; }
        public string ResponsibleId { get; set; }

        public string StatusChangeComment { get; set; }
     
    }
}
