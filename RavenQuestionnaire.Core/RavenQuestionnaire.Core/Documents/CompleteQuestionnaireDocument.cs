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
        public UserLight Creator { get; set; }

        public DateTime CreationDate { get; set; }
        public DateTime LastEntryDate { get; set; }

        public List<CompleteAnswer> CompletedAnswers { get; set; }

        public SurveyStatus Status { set; get; }

        public UserLight Responsible { get; set; }

        public string StatusChangeComment { get; set; }
     
    }
}
