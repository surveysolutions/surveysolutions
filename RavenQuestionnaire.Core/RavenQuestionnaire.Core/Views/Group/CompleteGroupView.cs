using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.ExpressionExecutors;
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
        public CompleteGroupView(CompleteQuestionnaireDocument doc, RavenQuestionnaire.Core.Entities.SubEntities.Group group, CompleteQuestionView[] questions)
            : this()
        {
            this.completeQuestionnaireDocument = doc;
            this.PublicKey = group.PublicKey;
            this.GroupText = group.GroupText;
            this._questions = questions;
            MerdgeAnswersWithResults();
        }

        protected CompleteQuestionnaireDocument completeQuestionnaireDocument;
        private CompleteQuestionView[] _questions;
        
       

        public Guid PublicKey { get; set; }
        public string GroupText { get; set; }
        public Guid? ParentGroup { get; set; }
        public string QuestionnaireId
        {
            get { return IdUtil.ParseId(this.completeQuestionnaireDocument.Id); }
        }
        public CompleteGroupView[] Groups { get; set; }
        public CompleteQuestionView[] Questions
        {
            get { return _questions; }
        }

        protected void MerdgeAnswersWithResults()
        {
            foreach (var answer in Questions.SelectMany(q => q.Answers))
            {
                var completeAnswer = completeQuestionnaireDocument.CompletedAnswers.FirstOrDefault(a => a.PublicKey.Equals(answer.PublicKey));
                if (completeAnswer != null)
                {
                    answer.Selected = true;
                    answer.CustomAnswer = completeAnswer.CustomAnswer;
                }
            }
        }
        
    }
}
