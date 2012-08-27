using System;
using System.Linq;
using RavenQuestionnaire.Core.AbstractFactories;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Views.Group;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire
{
    public class CompleteQuestionnaireViewEnumerable
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastEntryDate { get; set; }

        public SurveyStatus Status { get; set; }

        public UserLight Responsible { set; get; }
        public CompleteGroupView CurrentGroup { get; set; }
        public CompleteGroupView[] Groups { get; set; }

        public CompleteQuestionnaireViewEnumerable(CompleteQuestionnaireStoreDocument doc,
                                                   ICompleteGroup currentGroup, ICompleteGroupFactory groupFactory)
        {
            this.GroupFactory = groupFactory;
            this.Id = doc.PublicKey.ToString();
            this.Title = doc.Title;
            this.CreationDate = doc.CreationDate;
            this.LastEntryDate = doc.LastEntryDate;
            this.Status = doc.Status;
            this.Responsible = doc.Responsible;
            InitGroups(doc);
            this.CurrentGroup = GroupFactory.CreateGroup(doc, currentGroup);
        }
        public CompleteQuestionnaireViewEnumerable(CompleteQuestionnaireStoreDocument doc, ICompleteGroupFactory groupFactory)
        {
            this.GroupFactory = groupFactory;
            this.Title = doc.Title;
            CompleteGroup group = new CompleteGroup(){Children = doc.Children.Where(c=>c is ICompleteQuestion).ToList()};
            this.CurrentGroup = GroupFactory.CreateGroup(doc,group);
            InitGroups(doc);
        }

        protected readonly ICompleteGroupFactory GroupFactory;

        protected void InitGroups(CompleteQuestionnaireStoreDocument doc)
        {
            var questions = doc.Children.OfType<ICompleteQuestion>().ToList();
            var groups = doc.Children.OfType<ICompleteGroup>().ToList();
            if (questions.Count > 0)
            {
                this.Groups = new CompleteGroupView[groups.Count + 1];
                this.Groups[0] = GroupFactory.CreateGroup(doc, new CompleteGroup("Main") { PublicKey = Guid.Empty });
                for (int i = 1; i <= groups.Count; i++)
                {
                    this.Groups[i] = GroupFactory.CreateGroup(doc, groups[i - 1]);
                }
            }
            else
            {
                this.Groups = new CompleteGroupView[groups.Count];
                for (int i = 0; i < groups.Count; i++)
                {
                    this.Groups[i] = GroupFactory.CreateGroup(doc, groups[i]);
                }
            }
        }
    }
}