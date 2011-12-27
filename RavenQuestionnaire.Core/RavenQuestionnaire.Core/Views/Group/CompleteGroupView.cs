using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.Question;

namespace RavenQuestionnaire.Core.Views.Group
{
    public class CompleteGroupView
    {
        public CompleteGroupView()
        {
            Groups = new CompleteGroupView[0];
            _questions = new CompleteQuestionView[0];
        }
        public CompleteGroupView(CompleteQuestionnaireDocument doc, RavenQuestionnaire.Core.Entities.SubEntities.Group group)
            : this()
        {
            this.QuestionnaireId = doc.Id;
            this.PublicKey = group.PublicKey;
            this.GroupText = group.GroupText;
            this._questions =
               group.Questions.Select(q => new CompleteQuestionView(q, doc.Id)).ToArray();
            this.CompleteAnswers = doc.CompletedAnswers.ToArray();
            MerdgeAnswersWithResults();
        }

        public Guid PublicKey { get; set; }
        public string GroupText { get; set; }
        public Guid? ParentGroup { get; set; }

        public string QuestionnaireId
        {
            get { return IdUtil.ParseId(_questionnaireId); }
            set { _questionnaireId = value; }
        }
        private string _questionnaireId;

        public CompleteGroupView[] Groups { get; set; }
        public CompleteQuestionView[] Questions
        {
            get { return _questions; }
        }
        protected CompleteAnswer[] CompleteAnswers { get; set; }
        private CompleteQuestionView[] _questions;
        protected void MerdgeAnswersWithResults()
        {
            foreach (var answer in Questions.SelectMany(q => q.Answers))
            {
                var completeAnswer = CompleteAnswers.Where(a => a.PublicKey.Equals(answer.PublicKey)).FirstOrDefault();
                if (completeAnswer != null)
                {
                    answer.Selected = true;
                    answer.CustomAnswer = completeAnswer.CustomAnswer;
                }
            }
        }
    }
}
