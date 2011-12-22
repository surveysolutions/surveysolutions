using System;
using System.Collections.Generic;
using System.Linq;
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

        public QuestionView(string questionnaireId, RavenQuestionnaire.Core.Entities.SubEntities.Question doc)
        {
            this.PublicKey = doc.PublicKey;
            this.QuestionText = doc.QuestionText;
            this.QuestionType = doc.QuestionType;
            this.QuestionnaireId = questionnaireId;
            this.ConditionExpression = doc.ConditionExpression;
            this.Answers = doc.Answers.Select(a => new AnswerView(doc.PublicKey, a)).ToArray();
        }

        
    }
}
