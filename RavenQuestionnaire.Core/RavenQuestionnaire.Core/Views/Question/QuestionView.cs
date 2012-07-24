using System;
using System.Linq;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Views.Answer;
using System.ComponentModel.DataAnnotations;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.SubEntities;

using RavenQuestionnaire.Core.Views.Card;

namespace RavenQuestionnaire.Core.Views.Question
{
    public abstract class AbstractQuestionView : ICompositeView
    {
        #region Properties

        public int Index { get; set; }
        public Guid PublicKey { get; set; }

        [Display(Prompt = "Type question here")]
        public string Title { get; set; }

        [Display(Prompt = "When this question is enabled?")]
        public string ConditionExpression { get; set; }

        [Display(Prompt = "When this question is valid?")]
        public string ValidationExpression { get; set; }

        public QuestionType QuestionType { get; set; }

        public bool Featured { get; set; }

        public bool Mandatory { get; set; }

        public Order AnswerOrder { get; set; }
        //remove when exportSchema will be done 
        public string StataExportCaption { get; set; }

        public string Instructions { get; set; }

        public string Comments { get; set; }

        public string QuestionnaireId
        {
            get { return IdUtil.ParseId(_questionnaireId); }
            set { _questionnaireId = value; }
        }

        private string _questionnaireId;

        public Guid? Parent { get; set; }

        public List<ICompositeView> Children { get; set; }

        #endregion

        #region Constructor

        public AbstractQuestionView()
        {

        }

        public AbstractQuestionView(string questionnaireId, Guid? groupPublicKey)
            : this()
        {
            this.QuestionnaireId = questionnaireId;
            this.Parent = groupPublicKey;
        }

        protected AbstractQuestionView(IQuestionnaireDocument questionnaire, IQuestion doc)
            : this()
        {
            this.PublicKey = doc.PublicKey;
            this.Title = doc.QuestionText;
            this.QuestionType = doc.QuestionType;
            this.QuestionnaireId = questionnaire.Id;
            this.ConditionExpression = doc.ConditionExpression;
            this.ValidationExpression = doc.ValidationExpression;
            this.StataExportCaption = doc.StataExportCaption;
            this.Instructions = doc.Instructions;
            this.Comments = doc.Comments;
            this.AnswerOrder = doc.AnswerOrder;
            this.Featured = doc.Featured;
            this.Mandatory = doc.Mandatory;
        }

        #endregion
    }

    public abstract class AbstractQuestionView<T> : AbstractQuestionView where T : AnswerView
    {
        #region Properties

        public CardView[] Cards { get; set; }

        public T[] Answers
        {
            get { return _answers; }
            set
            {
                _answers = value;
                if (this._answers == null)
                {
                    this._answers = new T[0];
                    return;
                }

                for (int i = 0; i < this._answers.Length; i++)
                {
                    this._answers[i].Index = i + 1;
                }

            }
        }

        private T[] _answers;

        #endregion

        #region Constructor

        public AbstractQuestionView()
            : base()
        {
            Answers = new T[0];
            Cards = new CardView[0];
        }

        public AbstractQuestionView(string questionnaireId, Guid? groupPublicKey)
            : this()
        {
            this.QuestionnaireId = questionnaireId;
            this.Parent = groupPublicKey;
        }

        public AbstractQuestionView(IQuestionnaireDocument questionnaire, IQuestion doc)
            : base(questionnaire, doc)
        {
            Answers = new T[0];
            Cards = new CardView[0];
        }

        #endregion
    }

    public abstract class QuestionView<T, TGroup, TQuestion> : AbstractQuestionView<T>
        where T: AnswerView
        where TQuestion : IQuestion
        where TGroup : IGroup
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
            IQuestionnaireDocument questionnaire, TQuestion doc)
            :
                base(questionnaire, doc)
        {
            
            this.Parent = GetQuestionGroup(questionnaire, doc.PublicKey);
        }

        protected Guid? GetQuestionGroup(IQuestionnaireDocument questionnaire, Guid questionKey)
        {
            if (questionnaire.Children.Any(q => q.PublicKey.Equals(questionKey)))
                return null;
            var group = new Queue<IComposite>();
            foreach (var child in questionnaire.Children)
            {
                group.Enqueue(child);
            }
            while (group.Count != 0)
            {
                var queueItem = group.Dequeue();
                if (queueItem.Children != null)
                {
                    if (queueItem.Children.Any(q => q.PublicKey.Equals(questionKey)))
                        return queueItem.PublicKey;
                    foreach (var child in queueItem.Children)
                    {
                        /* var childWithQuestion = child as IGroup<IGroup, TQuestion>;
                         if(childWithQuestion!=null)*/
                        group.Enqueue(child);
                    }
                }
            }
            throw new ArgumentException("group does not exist");
        }
    }

    public class QuestionView : QuestionView<AnswerView, Entities.SubEntities.Group, IQuestion>
    {

        public QuestionView()
        {
        }

        public QuestionView(string questionnaireId, Guid? groupPublicKey)
            : base(questionnaireId, groupPublicKey)
        {
        }

        public QuestionView(
            IQuestionnaireDocument questionnaire,
            IQuestion doc)
            :
                base(questionnaire, doc)
        {
            this.Answers = doc.Children.Where(a=>a is IAnswer).Select(a => new AnswerView(doc.PublicKey, a as IAnswer)).ToArray();
            if (doc.Cards != null)
                this.Cards =
                    doc.Cards.Select(c => new CardView(doc.PublicKey, c)).OrderBy(a => Guid.NewGuid()).ToArray();

        }
    }
}
