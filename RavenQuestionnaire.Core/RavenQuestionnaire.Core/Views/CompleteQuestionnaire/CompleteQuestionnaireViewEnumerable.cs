using System;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.ExpressionExecutors;
using RavenQuestionnaire.Core.Utility;
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
        public GroupView[] Groups { get; set; }

        public CompleteQuestionnaireViewEnumerable(CompleteQuestionnaireDocument doc,
                                                   RavenQuestionnaire.Core.Entities.SubEntities.Group currentGroup, IExpressionExecutor<CompleteQuestionnaireDocument> executor)
        {
            this.Id = IdUtil.ParseId(doc.Id);
            this.Title = doc.Questionnaire.Title;
            this.CreationDate = doc.CreationDate;
            this.LastEntryDate = doc.LastEntryDate;
            this.Status = doc.Status;
            this.Responsible = doc.Responsible;
            InitGroups(doc.Questionnaire);

            if (currentGroup != null)
            {
                this.CurrentGroup = new CompleteGroupView(doc, currentGroup, executor);
            }

        }
        public CompleteQuestionnaireViewEnumerable(QuestionnaireDocument template, IExpressionExecutor<CompleteQuestionnaireDocument> executor)
        {
            this.Title = template.Title;
            this.CurrentGroup = new CompleteGroupView(new CompleteQuestionnaireDocument() {Questionnaire = template},
                                                      new Entities.SubEntities.Group() {Questions = template.Questions},
                                                      executor);
            InitGroups(template);
            /*  this.CurrentQuestion = new CompleteQuestionView(template.Questions[0], template.Id);*/
        }

        protected void InitGroups(QuestionnaireDocument doc)
        {
            if (doc.Questions.Count > 0)
            {
                this.Groups = new GroupView[doc.Groups.Count + 1];
                this.Groups[0] = new GroupView(doc,
                                               new Entities.SubEntities.Group("Main") {PublicKey = Guid.Empty});
                for (int i = 1; i <= doc.Groups.Count; i++)
                {
                    this.Groups[i] = new GroupView(doc, doc.Groups[i - 1]);
                }
            }
            else
            {
                this.Groups = new GroupView[doc.Groups.Count];
                for (int i = 0; i < doc.Groups.Count; i++)
                {
                    this.Groups[i] = new GroupView(doc, doc.Groups[i - 1]);
                }
            }
        }
    }
}
