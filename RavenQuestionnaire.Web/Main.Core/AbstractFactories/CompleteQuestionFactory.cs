// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionFactory.cs" company="The World Bank">
//   2012
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Core.AbstractFactories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Entities.Composite;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;
    using Main.Core.Entities.SubEntities.Complete.Question;
    using Main.Core.Entities.SubEntities.Question;
    using Main.Core.Events.Questionnaire;
    using Main.Core.Utility.OrderStrategy;

    /// <summary>
    /// The complete question factory.
    /// </summary>
    public class CompleteQuestionFactory : ICompleteQuestionFactory
    {
        #region Public Methods and Operators

        /// <summary>
        /// The convert to complete question.
        /// </summary>
        /// <param name="question">
        /// The question.
        /// </param>
        /// <returns>
        /// The Main.Core.Entities.SubEntities.Complete.ICompleteQuestion.
        /// </returns>
        public ICompleteQuestion ConvertToCompleteQuestion(IQuestion question)
        {
            var bindedQuestion = question as BindedQuestion;
            if (bindedQuestion != null)
            {
                return (BindedCompleteQuestion)bindedQuestion;
            }

            var template = question as IBinded;
            if (template != null)
            {
                return new BindedCompleteQuestion(question.PublicKey, template);
            }

            AbstractCompleteQuestion completeQuestion;
            if (question is IMultyOptionsQuestion)
            {
                completeQuestion = new MultyOptionsCompleteQuestion();
            }
            else if (question is ISingleQuestion)
            {
                completeQuestion = new SingleCompleteQuestion();
            }
            else if (question is IDateTimeQuestion)
            {
                completeQuestion = new DateTimeCompleteQuestion();
            }
            else if (question is INumericQuestion)
            {
                completeQuestion = new NumericCompleteQuestion();
            }
            else if (question is IAutoPropagate)
            {
                completeQuestion = new AutoPropagateCompleteQuestion(question as IAutoPropagate);
            }
            else if (question is IGpsCoordinatesQuestion)
            {
                completeQuestion = new GpsCoordinateCompleteQuestion();
            }
            else
            {
                completeQuestion = new TextCompleteQuestion();
            }

            completeQuestion.PublicKey = question.PublicKey;
            this.UpdateQuestion(
                completeQuestion, 
                question.QuestionType, 
                question.QuestionText, 
                question.StataExportCaption, 
                question.ConditionExpression, 
                question.ValidationExpression, 
                question.ValidationMessage, 
                question.AnswerOrder, 
                question.Featured, 
                question.Mandatory, 
                question.Capital, 
                question.Instructions, 
                null);
            completeQuestion.Comments = question.Comments;
            completeQuestion.Valid = true;

            IEnumerable<IComposite> ansersToCopy =
                new OrderStrategyFactory().Get(completeQuestion.AnswerOrder).Reorder(question.Children);
            if (ansersToCopy != null)
            {
                foreach (IAnswer composite in ansersToCopy)
                {
                    IComposite newAnswer;
                    if (question is ICompleteQuestion)
                    {
                        newAnswer = new CompleteAnswer(
                            composite as CompleteAnswer, ((ICompleteQuestion)question).PropogationPublicKey);
                    }
                    else
                    {
                        newAnswer = (CompleteAnswer)(composite as Answer);
                    }

                    completeQuestion.Children.Add(newAnswer);
                }
            }

            if (question.Cards != null)
            {
                foreach (Image card in question.Cards)
                {
                    completeQuestion.Cards.Add(card);
                }
            }

            return completeQuestion;
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// The Main.Core.Entities.SubEntities.AbstractQuestion.
        /// </returns>
        public AbstractQuestion Create(QuestionType type)
        {
            switch (type)
            {
                case QuestionType.MultyOption:
                    return new MultyOptionsQuestion();
                case QuestionType.DropDownList:
                    return new SingleQuestion();
                case QuestionType.SingleOption:
                    return new SingleQuestion();
                case QuestionType.YesNo:
                    return new SingleQuestion();
                case QuestionType.Text:
                    return new TextQuestion();
                case QuestionType.DateTime:
                    return new DateTimeQuestion();
                case QuestionType.Numeric:
                    return new NumericQuestion();
                case QuestionType.AutoPropagate:
                    return new AutoPropagateQuestion();
                case QuestionType.GpsCoordinates:
                    return new GpsCoordinateQuestion();
            }

            return null;
        }

        /// <summary>
        /// The update question by event.
        /// </summary>
        /// <param name="question">
        /// The question.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        public void UpdateQuestionByEvent(IQuestion question, NewQuestionAdded e)
        {
            this.UpdateQuestion(
                question, 
                e.QuestionType, 
                e.QuestionText, 
                e.StataExportCaption, 
                e.ConditionExpression, 
                e.ValidationExpression, 
                e.ValidationMessage, 
                e.AnswerOrder, 
                e.Featured, 
                e.Mandatory, 
                false, 
                e.Instructions, 
                e.Triggers);
            this.UpdateAnswerList(e.Answers, question);
        }

        /// <summary>
        /// The update question by event.
        /// </summary>
        /// <param name="question">
        /// The question.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        public void UpdateQuestionByEvent(IQuestion question, QuestionChanged e)
        {
            this.UpdateQuestion(
                question, 
                e.QuestionType, 
                e.QuestionText, 
                e.StataExportCaption, 
                e.ConditionExpression, 
                e.ValidationExpression, 
                e.ValidationMessage, 
                e.AnswerOrder, 
                e.Featured, 
                e.Mandatory, 
                false, 
                e.Instructions, 
                e.Triggers);
            this.UpdateAnswerList(e.Answers, question);
        }

        #endregion

        #region Methods

        /// <summary>
        /// The update answer list.
        /// </summary>
        /// <param name="answers">
        /// The answers.
        /// </param>
        /// <param name="question">
        /// The question.
        /// </param>
        protected void UpdateAnswerList(IEnumerable<Answer> answers, IQuestion question)
        {
            List<Answer> enumerable = (answers ?? new List<Answer>()).ToList();
            if (answers != null && enumerable.Any())
            {
                question.Children.Clear();
                foreach (Answer answer in enumerable)
                {
                    question.Add(answer, question.PublicKey);
                }
            }
        }

        /// <summary>
        /// The update question.
        /// </summary>
        /// <param name="question">
        /// The question.
        /// </param>
        /// <param name="questionType">
        /// The question type.
        /// </param>
        /// <param name="questionText">
        /// The question text.
        /// </param>
        /// <param name="stataExportCaption">
        /// The stata export caption.
        /// </param>
        /// <param name="conditionExpression">
        /// The condition expression.
        /// </param>
        /// <param name="validationExpression">
        /// The validation expression.
        /// </param>
        /// <param name="validationMessage">
        /// The validation message.
        /// </param>
        /// <param name="answerOrder">
        /// The answer order.
        /// </param>
        /// <param name="featured">
        /// The featured.
        /// </param>
        /// <param name="mandatory">
        /// The mandatory.
        /// </param>
        /// <param name="capital">
        /// The capital.
        /// </param>
        /// <param name="instructions">
        /// The instructions.
        /// </param>
        /// <param name="triggers">
        /// The triggers.
        /// </param>
        protected void UpdateQuestion(
            IQuestion question, 
            QuestionType questionType, 
            string questionText, 
            string stataExportCaption, 
            string conditionExpression, 
            string validationExpression, 
            string validationMessage, 
            Order answerOrder, 
            bool featured, 
            bool mandatory, 
            bool capital, 
            string instructions, 
            IEnumerable<Guid> triggers)
        {
            question.QuestionType = questionType;
            question.QuestionText = questionText;
            question.StataExportCaption = stataExportCaption;
            question.ConditionExpression = conditionExpression;
            question.ValidationExpression = validationExpression;
            question.ValidationMessage = validationMessage;
            question.AnswerOrder = answerOrder;
            question.Featured = featured;
            question.Mandatory = mandatory;
            question.Instructions = instructions;
            question.Capital = capital;

            var autoQuestion = question as IAutoPropagate;
            if (autoQuestion != null && triggers != null)
            {
                question.Triggers = new List<Guid>();
                foreach (var guid in triggers)
                {
                    question.Triggers.Add(guid);
                }
            }
        }

        #endregion
    }
}