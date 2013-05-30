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
            UpdateQuestion(
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
        public AbstractQuestion Create(FullQuestionDataEvent e)
        {
            AbstractQuestion q = CreateQuestion(e.QuestionType);

            q.PublicKey = e.PublicKey;

            UpdateQuestion(
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

            UpdateAnswerList(e.Answers, q);

            return q;
        }

        public IQuestion CreateQuestionFromExistingUsingDataFromEvent(IQuestion question, QuestionChanged e)
        {
            AbstractQuestion q = CreateQuestion(e.QuestionType);

            q.PublicKey = question.PublicKey;

            UpdateQuestion(
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

            UpdateAnswerList(e.Answers, q);

            return q;
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
        private static AbstractQuestion CreateQuestion(QuestionType type)
        {
            switch (type)
            {
                case QuestionType.MultyOption:
                    return new MultyOptionsQuestion();

                case QuestionType.DropDownList:
                case QuestionType.SingleOption:
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

                default:
                    throw new NotSupportedException(string.Format("Question type is not supported: {0}", type));
            }
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
        private static void UpdateAnswerList(IEnumerable<IAnswer> answers, IQuestion question)
        {
            if (question.Answers != null)
            {
                question.Answers.Clear();
            }

            if (answers != null && answers.Any())
            {
                foreach (var answer in answers)
                {
                    question.AddAnswer(answer);
                }
            }
        }

        private static void UpdateQuestion(
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
        }

        #endregion
    }
}