using System;
using System.Collections.Generic;
using System.Linq;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.Answer;

namespace RavenQuestionnaire.Core.Views.Question
{
    public class QuestionView
    {
        public int Index { get; set; }

        public Guid PublicKey { get; set; }

        public string QuestionText { get; set; }
        public string ConditionExpression { get; set; }
        public QuestionType QuestionType { get; set; }

        //remove when exportSchema will be done 
        public string StataExportCaption { get; set; }


        public AnswerView[] Answers
        {
            get { return _answers; }
            set
            {
                _answers = value;
                for (int i = 0; i < this._answers.Length; i++)
                {
                    this._answers[i].Index = i + 1;
                }

            }
        }

        private AnswerView[] _answers;

        public string QuestionnaireId
        {
            get { return IdUtil.ParseId(_questionnaireId); }
            set { _questionnaireId = value; }
        }

        public Guid? GroupPublicKey { get; set; }
        private string _questionnaireId;
        public QuestionView()
        {
            Answers = new AnswerView[0];
        }
        public QuestionView(string questionnaireId, Guid? groupPublicKey):this()
        {
            this.QuestionnaireId = questionnaireId;
            this.GroupPublicKey = groupPublicKey;
        }

        public QuestionView(QuestionnaireDocument questionnaire, Entities.SubEntities.Question doc)
        {
            this.PublicKey = doc.PublicKey;
            this.QuestionText = doc.QuestionText;
            this.QuestionType = doc.QuestionType;
            this.QuestionnaireId = questionnaire.Id;
            this.ConditionExpression = doc.ConditionExpression;
            this.Answers = doc.Answers.Select(a => new AnswerView(doc.PublicKey, a)).ToArray();
            this.GroupPublicKey = GetQuestionGroup(questionnaire, doc.PublicKey);
            this.StataExportCaption = doc.StataExportCaption;
        
        }
        protected Guid? GetQuestionGroup(QuestionnaireDocument questionnaire, Guid questionKey)
        {
            if (questionnaire.Questions.Where(q => q.PublicKey.Equals(questionKey)).Count() > 0)
                return null;
            Queue<Entities.SubEntities.Group> group= new Queue<Entities.SubEntities.Group>();
            foreach (var child in questionnaire.Groups)
            {
                group.Enqueue(child);
            }
            while (group.Count!=0)
            {
                var queueItem = group.Dequeue();

                if (queueItem.Questions.Where(q => q.PublicKey.Equals(questionKey)).Count() > 0)
                    return queueItem.PublicKey;
                foreach (var child in queueItem.Groups)
                {
                    group.Enqueue(child);
                }
            }
            throw new ArgumentException("group does not exist");
        }

    }
}
