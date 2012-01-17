using System;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Documents
{
    public interface IQuestionnaireDocument
    {
        string Id { get; set; }
        string Title { get; set; }
        DateTime CreationDate { get; set; }
        DateTime LastEntryDate { get; set; }
        DateTime? OpenDate { get; set; }
        DateTime? CloseDate { get; set; }

    }
    public interface IQuestionnaireDocument<TGroup, TQuestion> : IQuestionnaireDocument
        where TQuestion : IQuestion
        where TGroup : IGroup
    {
        List<TQuestion> Questions { get; set; }
        List<TGroup> Groups { get; set; }
    }

    public class QuestionnaireDocument : IQuestionnaireDocument<Group, Question>
    {
        public QuestionnaireDocument()
        {
            CreationDate = DateTime.Now;
            LastEntryDate = DateTime.Now;
            Questions = new List<Question>();
            Groups = new List<Group>();
            FlowGraph = null;
        }

        public string Id { get; set; }

        public string Title { get; set; }

        public DateTime CreationDate
        { get; set; }
        public DateTime LastEntryDate
        { get; set; }

        public DateTime? OpenDate
        { get; set; }

        public DateTime? CloseDate { get; set; }

        public List<Question> Questions { get; set; }
        public List<Group> Groups { get; set; }
        public FlowGraph FlowGraph { get; set; }
    }
}
