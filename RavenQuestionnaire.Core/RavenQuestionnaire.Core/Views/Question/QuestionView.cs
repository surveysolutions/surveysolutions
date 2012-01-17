using System;
using System.Collections.Generic;
using System.Linq;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.Answer;

namespace RavenQuestionnaire.Core.Views.Question
{
    public abstract class AbstractQuestionView
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

        private string _questionnaireId;

        public Guid? GroupPublicKey { get; set; }

        public AbstractQuestionView()
        {
            Answers = new AnswerView[0];
        }

        public AbstractQuestionView(string questionnaireId, Guid? groupPublicKey)
            : this()
        {
            this.QuestionnaireId = questionnaireId;
            this.GroupPublicKey = groupPublicKey;
        }

        public AbstractQuestionView(IQuestionnaireDocument questionnaire, IQuestion doc)
            : this()
        {
            this.PublicKey = doc.PublicKey;
            this.QuestionText = doc.QuestionText;
            this.QuestionType = doc.QuestionType;
            this.QuestionnaireId = questionnaire.Id;
            this.ConditionExpression = doc.ConditionExpression;
            this.StataExportCaption = doc.StataExportCaption;
        }
    }

    public abstract class QuestionView<TGroup, TQuestion, TAnswer> : AbstractQuestionView
        where TAnswer : IAnswer
        where TQuestion : IQuestion<TAnswer>
        where TGroup : IGroup<TGroup, TQuestion>
    {
        public QuestionView()
        {
        }
        public QuestionView(string questionnaireId, Guid? groupPublicKey)
            : base(questionnaireId, groupPublicKey)
        {
        }
        protected QuestionView(IQuestionnaireDocument questionnaire, IQuestion doc)
            : base(questionnaire, doc)
        {
        }

        public QuestionView(
            IQuestionnaireDocument
                <TGroup, TQuestion> questionnaire, TQuestion doc)
            :
                base(questionnaire, doc)
        {
            this.Answers = doc.Answers.Select(a => new AnswerView(doc.PublicKey, a)).ToArray();
            this.GroupPublicKey = GetQuestionGroup(questionnaire, doc.PublicKey);
        }

        protected Guid? GetQuestionGroup(IQuestionnaireDocument<TGroup, TQuestion> questionnaire, Guid questionKey)
        {
            if (questionnaire.Questions.Any(q => q.PublicKey.Equals(questionKey)))
                return null;
            var group = new Queue<TGroup>();
            foreach (var child in questionnaire.Groups)
            {
                group.Enqueue(child);
            }
            while (group.Count != 0)
            {
                var queueItem = group.Dequeue();

                if (queueItem.Questions.Any(q => q.PublicKey.Equals(questionKey)))
                    return queueItem.PublicKey;
                foreach (var child in queueItem.Groups)
                {
                    group.Enqueue(child);
                }
            }
            throw new ArgumentException("group does not exist");
        }
    }

    public class QuestionView :
        QuestionView
            <RavenQuestionnaire.Core.Entities.SubEntities.Group, RavenQuestionnaire.Core.Entities.SubEntities.Question,
            RavenQuestionnaire.Core.Entities.SubEntities.Answer>
    {
        public QuestionView()
        {
        }

        public QuestionView(string questionnaireId, Guid? groupPublicKey)
            : base(questionnaireId, groupPublicKey)
        {
        }

        protected QuestionView(IQuestionnaireDocument questionnaire, IQuestion doc)
            : base(questionnaire, doc)
        {
        }

        public QuestionView(
            IQuestionnaireDocument
                <RavenQuestionnaire.Core.Entities.SubEntities.Group,
                RavenQuestionnaire.Core.Entities.SubEntities.Question> questionnaire,
            RavenQuestionnaire.Core.Entities.SubEntities.Question doc)
            :
                base(questionnaire, doc)
        {
            this.Answers = doc.Answers.Select(a => new AnswerView(doc.PublicKey, a)).ToArray();
            this.GroupPublicKey = GetQuestionGroup(questionnaire, doc.PublicKey);
        }
    }
}
