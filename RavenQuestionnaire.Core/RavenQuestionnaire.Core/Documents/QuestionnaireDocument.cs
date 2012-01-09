using System;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Documents
{
    public class QuestionnaireDocument
    {
        public QuestionnaireDocument()
        {
            CreationDate = DateTime.Now;
            LastEntryDate = DateTime.Now;
            Questions= new List<Question>();
            Groups= new List<Group>();
        }

        public string Id { get; set; }

        public string Title
        { get; set; }

        public DateTime CreationDate
        { get; set; }
        public DateTime LastEntryDate
        { get; set; }

        public DateTime? OpenDate
        { get; set; }

        public DateTime? CloseDate
        { get; set; }
        public List<Question> Questions { get; set; }
        public List<Group> Groups { get; set; }
    }
}
