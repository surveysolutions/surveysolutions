using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Documents
{
    public interface IQuestionnaireDocument : IGroup
    {
        string Id { get; set; }
        DateTime CreationDate { get; set; }
        DateTime LastEntryDate { get; set; }
        DateTime? OpenDate { get; set; }
        DateTime? CloseDate { get; set; }

    }

    public interface IQuestionnaireDocument<TGroup, TQuestion> : IQuestionnaireDocument, IGroup<TGroup, TQuestion>
        where TQuestion : IQuestion
        where TGroup : IGroup
    {
    }

    public class QuestionnaireDocument : IQuestionnaireDocument<IGroup, IQuestion>
    {
        public QuestionnaireDocument()
        {
            CreationDate = DateTime.Now;
            LastEntryDate = DateTime.Now;
            Questions = new List<IQuestion>();
            Groups = new List<IGroup>();
            FlowGraph = null;
        }

        public string Id { get; set; }
       
        public string Title { get; set; }
        [XmlIgnore]
        public Guid PublicKey { get; set; }
        [XmlIgnore]
        public bool Propagated
        {
            get { return false; }
            set {  }
        }

        public DateTime CreationDate
        { get; set; }
        public DateTime LastEntryDate
        { get; set; }

        public DateTime? OpenDate
        { get; set; }

        public DateTime? CloseDate { get; set; }

        public List<IQuestion> Questions { get; set; }
        public List<IGroup> Groups { get; set; }
        public FlowGraph FlowGraph { get; set; }

        
    }
}
