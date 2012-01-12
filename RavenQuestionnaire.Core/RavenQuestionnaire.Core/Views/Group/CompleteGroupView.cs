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
        public CompleteGroupView(string completeQuestionnaireId, Entities.SubEntities.Complete.CompleteGroup group)
            : this()
        {
            this.completeQuestionnaireId = IdUtil.ParseId(completeQuestionnaireId);
            this._questions =
                group.Questions.Select(q => new CompleteQuestionView(q, completeQuestionnaireId)).ToArray();
            this.PublicKey = group.PublicKey;
            this.GroupText = group.GroupText;
        }
        protected string completeQuestionnaireId;
        private readonly CompleteQuestionView[] _questions;

        public Guid PublicKey { get; set; }
        public string GroupText { get; set; }
        public Guid? ParentGroup { get; set; }
        public CompleteGroupView[] Groups { get; set; }

        public CompleteQuestionView[] Questions
        {
            get { return _questions; }
        }
        public string QuestionnaireId
        {
            get { return this.completeQuestionnaireId; }
        }
        
    }
}
