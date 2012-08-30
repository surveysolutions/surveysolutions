// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Questionnaire.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The questionnaire.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using RavenQuestionnaire.Core.AbstractFactories;
    using RavenQuestionnaire.Core.Documents;
    using RavenQuestionnaire.Core.Entities.Composite;
    using RavenQuestionnaire.Core.Entities.Extensions;
    using RavenQuestionnaire.Core.Entities.SubEntities;

    /// <summary>
    /// The questionnaire.
    /// </summary>
    public class Questionnaire : IEntity<QuestionnaireDocument>
    {
        #region Fields

        /// <summary>
        /// The inner document.
        /// </summary>
        private readonly QuestionnaireDocument innerDocument;

        #endregion

        // public string QuestionnaireId { get { return innerDocument.Id; } }
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Questionnaire"/> class.
        /// </summary>
        /// <param name="title">
        /// The title.
        /// </param>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        public Questionnaire(string title, Guid publicKey)
        {
            this.innerDocument = new QuestionnaireDocument { Title = title, PublicKey = publicKey };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Questionnaire"/> class.
        /// </summary>
        /// <param name="innerDocument">
        /// The inner document.
        /// </param>
        public Questionnaire(QuestionnaireDocument innerDocument)
        {
            this.innerDocument = innerDocument;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the public key.
        /// </summary>
        public Guid PublicKey
            result.Capital = capital;
        {
            get
            {
                return this.innerDocument.PublicKey;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The add.
        /// </summary>
        /// <param name="c">
        /// The c.
        /// </param>
        /// <param name="parent">
        /// The parent.
        /// </param>
        public void Add(IComposite c, Guid? parent)
        {
            this.innerDocument.Add(c, parent);
        }

        /// <summary>
        /// The add group.
        /// </summary>
        /// <param name="groupText">
        /// The group text.
        /// </param>
        /// <param name="propageted">
        /// The propageted.
        /// </param>
        /// <param name="triggers">
        /// The triggers.
        /// </param>
        /// <param name="parent">
        /// The parent.
        /// </param>
        /// <param name="conditionExpression">
        /// The condition expression.
        /// </param>
        /// <exception cref="ArgumentException">
        /// </exception>
        public void AddGroup(
            string groupText, Propagate propageted, List<Guid> triggers, Guid? parent, string conditionExpression)
        {
            var group = new Group();
            group.Title = groupText;
            group.Propagated = propageted;
            group.Triggers = triggers;
            group.ConditionExpression = conditionExpression;
            try
            {
                this.Add(group, parent);
            }
            catch (Exception)
            {
                throw new ArgumentException(string.Format("group with  publick key {0} can't be found", parent.Value));
            }
        }

        /// <summary>
        /// The add group.
        /// </summary>
        /// <param name="groupText">
        /// The group text.
        /// </param>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="propageted">
        /// The propageted.
        /// </param>
        /// <param name="parent">
        /// The parent.
        /// </param>
        /// <param name="conditionExpression">
        /// The condition expression.
        /// </param>
        /// <exception cref="ArgumentException">
        /// </exception>
        public void AddGroup(
            string groupText, Guid publicKey, Propagate propageted, Guid? parent, string conditionExpression)
        {
            var group = new Group();
            group.Title = groupText;
            group.Propagated = propageted;
            group.PublicKey = publicKey;
            group.ConditionExpression = conditionExpression;
            try
            {
                this.Add(group, parent);
            }
            catch (Exception)
            {
                throw new ArgumentException(string.Format("group with  publick key {0} can't be found", parent.Value));
            }
        }

        /// <summary>
        /// The add group.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="groupText">
        /// The group text.
        /// </param>
        /// <param name="propageted">
        /// The propageted.
        /// </param>
        /// <param name="triggers">
        /// The triggers.
        /// </param>
        /// <param name="parent">
        /// The parent.
        /// </param>
        /// <param name="conditionExpression">
        /// The condition expression.
        /// </param>
        /// <exception cref="ArgumentException">
        /// </exception>
        public void AddGroup(
            Guid publicKey, 
            string groupText, 
            Propagate propageted, 
            List<Guid> triggers, 
            Guid? parent, 
            string conditionExpression)
        {
            var group = new Group();
            group.Title = groupText;
            group.PublicKey = publicKey;
            group.Propagated = propageted;
            group.Triggers = triggers;
            group.ConditionExpression = conditionExpression;
            try
            {
                this.Add(group, parent);
            }
            catch (Exception)
            {
                throw new ArgumentException(string.Format("group with  publick key {0} can't be found", parent.Value));
            }
        }

        /// <summary>
        /// The add group.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="groupText">
        /// The group text.
        /// </param>
        /// <param name="propageted">
        /// The propageted.
        /// </param>
        /// <param name="parent">
        /// The parent.
        /// </param>
        /// <param name="conditionExpression">
        /// The condition expression.
        /// </param>
        /// <exception cref="ArgumentException">
        /// </exception>
        public void AddGroup(
            Guid publicKey, string groupText, Propagate propageted, Guid? parent, string conditionExpression)
        {
            var group = new Group();
            group.PublicKey = publicKey;
            group.Title = groupText;
            group.Propagated = propageted;
            group.ConditionExpression = conditionExpression;
            try
            {
                this.Add(group, parent);
            }
            catch (Exception)
            {
                throw new ArgumentException(string.Format("group with  publick key {0} can't be found", parent.Value));
            }
        }

        /// <summary>
        /// The add question.
        /// </summary>
        /// <param name="qid">
        /// The qid.
        /// </param>
        /// <param name="text">
        /// The text.
        /// </param>
        /// <param name="stataExportCaption">
        /// The stata export caption.
        /// </param>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="condition">
        /// The condition.
        /// </param>
        /// <param name="validation">
        /// The validation.
        /// </param>
        /// <param name="featured">
        /// The featured.
        /// </param>
        /// <param name="mandatory">
        /// The mandatory.
        /// </param>
        /// <param name="answerOrder">
        /// The answer order.
        /// </param>
        /// <param name="groupPublicKey">
        /// The group public key.
        /// </param>
        /// <param name="answers">
        /// The answers.
        /// </param>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <returns>
        /// The RavenQuestionnaire.Core.Entities.SubEntities.AbstractQuestion.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// </exception>
        public AbstractQuestion AddQuestion(
            Guid qid, 
            string text, 
            string stataExportCaption, 
            QuestionType type, 
            string condition, 
            string validation, 
            bool featured, 
            bool capital,
            bool mandatory, 
            Order answerOrder, 
            Guid? groupPublicKey, 
            IEnumerable<Answer> answers, 
            Guid publicKey)
        {
            AbstractQuestion result = new CompleteQuestionFactory().Create(type);
            result.PublicKey = qid;
            result.QuestionType = type;
            result.QuestionText = text;
            result.StataExportCaption = stataExportCaption;
            result.ConditionExpression = condition;
            result.ValidationExpression = validation;
            result.AnswerOrder = answerOrder;
            result.Featured = featured;
            result.Capital = capital;
            result.Mandatory = mandatory;
            result.PublicKey = publicKey;
            this.UpdateAnswerList(answers, result);
            try
            {
                this.Add(result, groupPublicKey);
                return result;
            }
            catch (Exception)
            {
                throw new ArgumentException(
                    string.Format("group with  publick key {0} can't be found", groupPublicKey.Value));
            }
        }

        /// <summary>
        /// The clear questions.
        /// </summary>
        public void ClearQuestions()
        {
            this.innerDocument.Children.RemoveAll(a => a is IQuestion);
        }

        /// <summary>
        /// The find.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The T.
        /// </returns>
        public T Find<T>(Guid publicKey) where T : class, IComposite
        {
            return this.innerDocument.Find<T>(publicKey);
        }

        /// <summary>
        /// The find.
        /// </summary>
        /// <param name="condition">
        /// The condition.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The System.Collections.Generic.IEnumerable`1[T -&gt; T].
        /// </returns>
        public IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class
        {
            return this.innerDocument.Find(condition);
        }

        /// <summary>
        /// The first or default.
        /// </summary>
        /// <param name="condition">
        /// The condition.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The T.
        /// </returns>
        public T FirstOrDefault<T>(Func<T, bool> condition) where T : class
        {
            return this.innerDocument.FirstOrDefault(condition);
        }

        /// <summary>
        /// The get all questions.
        /// </summary>
        /// <returns>
        /// The System.Collections.Generic.IList`1[T -&gt; RavenQuestionnaire.Core.Entities.SubEntities.IQuestion].
        /// </returns>
        public IList<IQuestion> GetAllQuestions()
        {
            return this.innerDocument.GetAllQuestions<IQuestion>().ToList();
        }

        /// <summary>
        /// The move item.
        /// </summary>
        /// <param name="itemPublicKey">
        /// The item public key.
        /// </param>
        /// <param name="groupKey">
        /// The group key.
        /// </param>
        /// <param name="after">
        /// The after.
        /// </param>
        /// <exception cref="ArgumentException">
        /// </exception>
        public void MoveItem(Guid itemPublicKey, Guid? groupKey, Guid? after)
        {
            bool result = this.MoveItem(this.innerDocument, itemPublicKey, groupKey, after);
            if (!result)
            {
                throw new ArgumentException(string.Format("item doesn't exists -{0}", itemPublicKey));
            }
        }

        /// <summary>
        /// The remove.
        /// </summary>
        /// <param name="c">
        /// The c.
        /// </param>
        public void Remove(IComposite c)
        {
            this.innerDocument.Remove(c);
        }

        /// <summary>
        /// The remove.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        public void Remove(Guid publicKey)
        {
            this.innerDocument.Remove(publicKey);
        }

        /// <summary>
        /// The update condition expression.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="condition">
        /// The condition.
        /// </param>
        public void UpdateConditionExpression(Guid publicKey, string condition)
        {
            var question = Find<AbstractQuestion>(publicKey);
            if (question == null)
            {
                return;
            }

            question.ConditionExpression = condition;
        }

        /// <summary>
        /// The update group.
        /// </summary>
        /// <param name="groupText">
        /// The group text.
        /// </param>
        /// <param name="propageted">
        /// The propageted.
        /// </param>
        /// <param name="triggers">
        /// The triggers.
        /// </param>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="conditionExpression">
        /// The condition expression.
        /// </param>
        /// <exception cref="ArgumentException">
        /// </exception>
        public void UpdateGroup(
            string groupText, Propagate propageted, List<Guid> triggers, Guid publicKey, string conditionExpression)
        {
            var group = Find<Group>(publicKey);
            if (group != null)
            {
                group.Propagated = propageted;
                group.Triggers = triggers;
                group.ConditionExpression = conditionExpression;
                group.Update(groupText);
                return;
            }

            throw new ArgumentException(string.Format("group with  publick key {0} can't be found", publicKey));
        }

        /// <summary>
        /// The update group.
        /// </summary>
        /// <param name="groupText">
        /// The group text.
        /// </param>
        /// <param name="propageted">
        /// The propageted.
        /// </param>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="conditionExpression">
        /// The condition expression.
        /// </param>
        /// <exception cref="ArgumentException">
        /// </exception>
        public void UpdateGroup(string groupText, Propagate propageted, Guid publicKey, string conditionExpression)
        {
            var group = Find<Group>(publicKey);
            if (group != null)
            {
                group.Propagated = propageted;
                group.ConditionExpression = conditionExpression;
                group.Update(groupText);
                return;
            }

            throw new ArgumentException(string.Format("group with  publick key {0} can't be found", publicKey));
        }

        /// <summary>
        /// The update question.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="text">
        /// The text.
        /// </param>
        /// <param name="stataExportCaption">
        /// The stata export caption.
        /// </param>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="condition">
        /// The condition.
        /// </param>
        /// <param name="validation">
        /// The validation.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="instructions">
        /// The instructions.
        /// </param>
        /// <param name="featured">
        /// The featured.
        /// </param>
        /// <param name="mandatory">
        /// The mandatory.
        /// </param>
        /// <param name="answerOrder">
        /// The answer order.
        /// </param>
        /// <param name="answers">
        /// The answers.
        /// </param>
        public void UpdateQuestion(
            Guid publicKey, 
            string text, 
            string stataExportCaption, 
            QuestionType type, 
            string condition, 
            string validation, 
            string message, 
            string instructions, 
            bool featured, 
            bool capital,
            bool mandatory, 
            Order answerOrder, 
            IEnumerable<Answer> answers)
        {
            var question = Find<AbstractQuestion>(publicKey);
            if (question == null)
            {
                return;
            }

            question.QuestionText = text;
            question.StataExportCaption = stataExportCaption;
            question.QuestionType = type;
            this.UpdateAnswerList(answers, question);
            question.ConditionExpression = condition;
            question.ValidationExpression = validation;
            question.ValidationMessage = message;
            question.Instructions = instructions;
            question.Featured = featured;
            question.Capital = capital;
            question.Mandatory = mandatory;
            question.AnswerOrder = answerOrder;
        }

        /// <summary>
        /// The update text.
        /// </summary>
        /// <param name="text">
        /// The text.
        /// </param>
        public void UpdateText(string text)
        {
            this.innerDocument.Title = text;
            this.innerDocument.LastEntryDate = DateTime.Now;
        }

        #endregion

        #region Explicit Interface Methods

        /// <summary>
        /// The get inner document.
        /// </summary>
        /// <returns>
        /// The RavenQuestionnaire.Core.Documents.QuestionnaireDocument.
        /// </returns>
        QuestionnaireDocument IEntity<QuestionnaireDocument>.GetInnerDocument()
        {
            return this.innerDocument;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The move.
        /// </summary>
        /// <param name="groups">
        /// The groups.
        /// </param>
        /// <param name="itemPublicKey">
        /// The item public key.
        /// </param>
        /// <param name="groupKey">
        /// The group key.
        /// </param>
        /// <param name="after">
        /// The after.
        /// </param>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// </exception>
        protected bool Move(List<IComposite> groups, Guid itemPublicKey, Guid? groupKey, Guid? after)
        {
            IComposite moveble = groups.FirstOrDefault(g => g.PublicKey == itemPublicKey);
            if (moveble == null)
            {
                return false;
            }

            if (groupKey.HasValue)
            {
                var moveToGroup = this.innerDocument.Find<Group>((Guid)groupKey);
                if (moveToGroup != null)
                {
                    groups.Remove(moveble);
                    moveToGroup.Insert(moveble, after);
                    return true;
                }
            }

            if (!after.HasValue)
            {
                groups.Remove(moveble);
                groups.Insert(0, moveble);
                return true;
            }

            for (int i = 0; i < groups.Count; i++)
            {
                if (groups[i].PublicKey == after.Value)
                {
                    groups.Remove(moveble);
                    if (i < groups.Count)
                    {
                        groups.Insert(i + 1, moveble);
                    }
                    else
                    {
                        groups.Add(moveble);
                    }

                    return true;
                }
            }

            throw new ArgumentException(string.Format("target item doesn't exists -{0}", after));
        }

        /// <summary>
        /// The move item.
        /// </summary>
        /// <param name="root">
        /// The root.
        /// </param>
        /// <param name="itemPublicKey">
        /// The item public key.
        /// </param>
        /// <param name="groupKey">
        /// The group key.
        /// </param>
        /// <param name="after">
        /// The after.
        /// </param>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        protected bool MoveItem(IComposite root, Guid itemPublicKey, Guid? groupKey, Guid? after)
        {
            if (this.Move(root.Children, itemPublicKey, groupKey, after))
            {
                return true;
            }

            return root.Children.Any(@group => this.MoveItem(group, itemPublicKey, groupKey, after));
        }

        /// <summary>
        /// The update answer list.
        /// </summary>
        /// <param name="answers">
        /// The answers.
        /// </param>
        /// <param name="question">
        /// The question.
        /// </param>
        protected void UpdateAnswerList(IEnumerable<Answer> answers, AbstractQuestion question)
        {
            if (answers != null && answers.Any())
            {
                question.Children.Clear();
                foreach (Answer answer in answers)
                {
                    question.Add(answer, question.PublicKey);
                }
            }
        }

        #endregion
    }
}