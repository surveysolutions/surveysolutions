using System;
using System.Linq;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.ExpressionExecutors;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.Group;
using RavenQuestionnaire.Core.Views.Question;

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

        public CompleteQuestionnaireViewEnumerable(CompleteQuestionnaireDocument doc,
                                                   CompleteGroup currentGroup)
        {
            this.Id = IdUtil.ParseId(doc.Id);
            this.Title = doc.Title;
            this.CreationDate = doc.CreationDate;
            this.LastEntryDate = doc.LastEntryDate;
            this.Status = doc.Status;
            this.Responsible = doc.Responsible;
            InitGroups(doc);
            this.CurrentGroup = new CompleteGroupView(doc.Id, currentGroup);
        }
        public CompleteQuestionnaireViewEnumerable(CompleteQuestionnaireDocument doc)
        {
            this.Title = doc.Title;
            Entities.SubEntities.Complete.CompleteGroup group = new Entities.SubEntities.Complete.CompleteGroup()
                                                                    {
                                                                        Questions =
                                                                            doc.Questions.Select(
                                                                                q =>
                                                                                new CompleteQuestion(q.QuestionText,
                                                                                                     q.QuestionType)).
                                                                            ToList()
                                                                    };
            this.CurrentGroup = new CompleteGroupView(null,
                                                      group);
            InitGroups(doc);
        }

        protected void InitGroups(CompleteQuestionnaireDocument doc)
        {
            if (doc.Questions.Count > 0)
            {
                this.Groups = new CompleteGroupView[doc.Groups.Count + 1];
                this.Groups[0] = new CompleteGroupView(doc.Id,
                                               new Entities.SubEntities.Complete.CompleteGroup("Main") {PublicKey = Guid.Empty});
                for (int i = 1; i <= doc.Groups.Count; i++)
                {
                    this.Groups[i] = new CompleteGroupView(doc.Id, doc.Groups[i - 1]);
                }
            }
            else
            {
                this.Groups = new CompleteGroupView[doc.Groups.Count];
                for (int i = 0; i < doc.Groups.Count; i++)
                {
                    this.Groups[i] = new CompleteGroupView(doc.Id, doc.Groups[i - 1]);
                }
            }
        }
    }
}
