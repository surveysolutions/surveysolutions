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

        private string _questionnaireId;
        public QuestionView()
        {
            Answers = new AnswerView[0];
        }
        public QuestionView(Guid publicKey, 
            string text, 
            QuestionType type, 
            string questionnaireId,
            string conditionExpression)
        {
            this.PublicKey = publicKey;
            this.QuestionText = text;
            this.QuestionType = type;
            this.QuestionnaireId = questionnaireId;
            this.ConditionExpression = conditionExpression;
        }

        public QuestionView(Guid publicKey, 
            string text, 
            QuestionType type, 
            IEnumerable<RavenQuestionnaire.Core.Entities.SubEntities.Answer> answers,
            string questionnaireId,
            string conditionExpression)
            : this(publicKey, text, type, questionnaireId, conditionExpression)
        {
            this.Answers = answers.Select(answer => new AnswerView(answer.PublicKey, answer.AnswerText, answer.Mandatory, answer.AnswerType, this.PublicKey)).ToArray();
           
        }
        public QuestionView(Guid publicKey, 
            string text, 
            QuestionType type, 
            IEnumerable<AnswerView> answers,
            string questionnaireId,
            string conditionExpression)
            : this(publicKey, text, type, questionnaireId, conditionExpression)
        {
           this.Answers = answers.ToArray();
        }
        public static QuestionView New(string questionnaireId)
        {
            return new QuestionView() {QuestionnaireId = questionnaireId};
        }
    }
}
