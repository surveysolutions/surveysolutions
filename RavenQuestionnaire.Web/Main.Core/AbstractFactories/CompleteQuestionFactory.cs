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
            var maxValue = int.MaxValue;
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
                maxValue = (question as IAutoPropagate).MaxValue;
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
                question.QuestionScope,
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
                null,
                maxValue);
            ////completeQuestion.Comments = question.Comments;
            completeQuestion.Valid = true;

            IEnumerable<IAnswer> answersToCopy =
                new OrderStrategyFactory().Get(completeQuestion.AnswerOrder).Reorder(question.Answers);

            if (answersToCopy != null)
            {
                foreach (IAnswer composite in answersToCopy)
                {
                    IAnswer newAnswer;
                    if (question is ICompleteQuestion)
                    {
                        newAnswer = new CompleteAnswer(
                            composite as CompleteAnswer, ((ICompleteQuestion)question).PropagationPublicKey);
                    }
                    else
                    {
                        newAnswer = (CompleteAnswer)(composite as Answer);
                    }

                    completeQuestion.AddAnswer(newAnswer);
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
        /// <param name="e">
        /// The e.
        /// </param>
        /// <returns>
        /// The <see cref="AbstractQuestion"/>.
        /// </returns>
        public AbstractQuestion Create(NewQuestionAdded e)
        {
            AbstractQuestion q = this.CreateQuestion(e.QuestionType);

            q.PublicKey = e.PublicKey;

            this.UpdateQuestion(
                q,
                e.QuestionType,
                e.QuestionScope,
                e.QuestionText,
                e.StataExportCaption,
                e.ConditionExpression,
                e.ValidationExpression,
                e.ValidationMessage,
                e.AnswerOrder,
                e.Featured,
                e.Mandatory,
                e.Capital,
                e.Instructions,
                e.Triggers,
                e.MaxValue);

            this.UpdateAnswerList(e.Answers, q);

            return q;
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
            // AbstractQuestion q = this.CreateQuestion(e.QuestionType);

            // q.PublicKey = question.PublicKey;

            this.UpdateQuestion(
                question,
                e.QuestionType,
                e.QuestionScope,
                e.QuestionText,
                e.StataExportCaption,
                e.ConditionExpression,
                e.ValidationExpression,
                e.ValidationMessage,
                e.AnswerOrder,
                e.Featured,
                e.Mandatory,
                e.Capital,
                e.Instructions,
                e.Triggers,
                e.MaxValue);

            this.UpdateAnswerList(e.Answers, question);

            //question = q;
        }

        /// <summary>
        /// The create question.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// The <see cref="AbstractQuestion"/>.
        /// </returns>
        private AbstractQuestion CreateQuestion(QuestionType type)
        {
            AbstractQuestion q = null;
            switch (type)
            {
                case QuestionType.MultyOption:
                    q = new MultyOptionsQuestion();
                    break;
                case QuestionType.DropDownList:
                    q = new SingleQuestion();
                    break;
                case QuestionType.SingleOption:
                    q = new SingleQuestion();
                    break;
                case QuestionType.YesNo:
                    q = new SingleQuestion();
                    break;
                case QuestionType.Text:
                    q = new TextQuestion();
                    break;
                case QuestionType.DateTime:
                    q = new DateTimeQuestion();
                    break;
                case QuestionType.Numeric:
                    q = new NumericQuestion();
                    break;
                case QuestionType.AutoPropagate:
                    q = new AutoPropagateQuestion();
                    break;
                case QuestionType.GpsCoordinates:
                    q = new GpsCoordinateQuestion();
                    break;
            }

            return q;
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
        private void UpdateAnswerList(IEnumerable<IAnswer> answers, IQuestion question)
        {
            if (answers != null && answers.Any())
            {
                question.Answers.Clear();
                foreach (var answer in answers)
                {
                    question.AddAnswer(answer);
                }
            }
        }

        private void UpdateQuestion(
            IQuestion question,
            QuestionType questionType,
            QuestionScope questionScope,
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
            IEnumerable<Guid> triggers,
            int maxValue)
        {
            question.QuestionType = questionType;
            question.QuestionScope = questionScope;
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
            if (autoQuestion != null)
            {
                autoQuestion.MaxValue = maxValue;
                if (triggers != null)
                {
                    autoQuestion.Triggers = new List<Guid>();
                    foreach (var guid in triggers)
                    {
                        if (!autoQuestion.Triggers.Contains(guid))
                        {
                            autoQuestion.Triggers.Add(guid);
                        }
                    }
                }
            }

            #endregion
        }
    }
}