using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using RavenQuestionnaire.Core.AbstractFactories;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.Documents
{
    public interface ICompleteQuestionnaireDocument<TGroup, TQuestion> : IQuestionnaireDocument<TGroup, TQuestion>, ICompleteGroup<TGroup, TQuestion>
        where TQuestion : ICompleteQuestion
        where TGroup : ICompleteGroup
    {
         UserLight Creator { get; set; }

         string TemplateId { get; set; }

         SurveyStatus Status { set; get; }

         UserLight Responsible { get; set; }

         string StatusChangeComment { get; set; }
    }


    public class CompleteQuestionnaireDocument : ICompleteQuestionnaireDocument<ICompleteGroup, ICompleteQuestion>
    {
        public CompleteQuestionnaireDocument()
        {
            CreationDate = DateTime.Now;
            LastEntryDate = DateTime.Now;
            Questions = new List<ICompleteQuestion>();
            Groups = new List<ICompleteGroup>();
        }
        public static explicit operator CompleteQuestionnaireDocument(QuestionnaireDocument doc)
        {
            CompleteQuestionnaireDocument result = new CompleteQuestionnaireDocument
            {
                TemplateId = doc.Id,
                Title = doc.Title
            };
            result.Questions =
                doc.Questions.Select(q => new CompleteQuestionFactory().ConvertToCompleteQuestion(q)).ToList();
            result.Groups =
                doc.Groups.Select(q => new CompleteGroupFactory().ConvertToCompleteGroup(q)).ToList();
            return result;
        }
        public UserLight Creator { get; set; }

        public string TemplateId { get; set; }

        public SurveyStatus Status { set; get; }

        public UserLight Responsible { get; set; }

        public string StatusChangeComment { get; set; }

        #region Implementation of IQuestionnaireDocument

        public List<ICompleteQuestion> Questions { get; set; }

        public List<ICompleteGroup> Groups { get; set; }

        public string Id { get; set; }

        public string Title { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastEntryDate { get; set; }

        public DateTime? OpenDate { get; set; }

        public DateTime? CloseDate { get; set; }

        [XmlIgnore]
        public Guid PublicKey { get; set; }
        [XmlIgnore]
        public bool Propagated
        {
            get { return false; }
            set { }
        }

        #endregion
    }
}
