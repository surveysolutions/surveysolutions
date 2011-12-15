using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities;
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
    }
}
