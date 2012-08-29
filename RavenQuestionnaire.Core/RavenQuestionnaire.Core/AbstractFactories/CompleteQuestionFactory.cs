// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   Defines the CompleteQuestionFactory type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.AbstractFactories
{
    using System.Collections.Generic;

    using RavenQuestionnaire.Core.Documents;
    using RavenQuestionnaire.Core.Entities.Composite;
    using RavenQuestionnaire.Core.Entities.SubEntities;
    using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
    using RavenQuestionnaire.Core.Entities.SubEntities.Complete.Question;
    using RavenQuestionnaire.Core.Entities.SubEntities.Question;
    using RavenQuestionnaire.Core.Utility.OrderStrategy;
    using RavenQuestionnaire.Core.Views.Question;

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
        /// The RavenQuestionnaire.Core.Entities.SubEntities.Complete.ICompleteQuestion.
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
            completeQuestion.ConditionExpression = question.ConditionExpression;
            completeQuestion.QuestionText = question.QuestionText;
            completeQuestion.QuestionType = question.QuestionType;
            completeQuestion.StataExportCaption = question.StataExportCaption;
            completeQuestion.Instructions = question.Instructions;
            completeQuestion.Comments = question.Comments;
            completeQuestion.Triggers = question.Triggers;
            completeQuestion.ValidationExpression = question.ValidationExpression;
            completeQuestion.ValidationMessage = question.ValidationMessage;
            completeQuestion.AnswerOrder = question.AnswerOrder;
            completeQuestion.Valid = true;
            completeQuestion.Featured = question.Featured;
            completeQuestion.Mandatory = question.Mandatory;

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
        /// The RavenQuestionnaire.Core.Entities.SubEntities.AbstractQuestion.
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

            return new TextQuestion();
        }

        /*public CompleteQuestionView CreateQuestion(CompleteQuestionnaireDocument doc, ICompleteQuestion question)
        {
            return new CompleteQuestionView(doc, question);
        }*/

        /// <summary>
        /// The create question.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <param name="question">
        /// The question.
        /// </param>
        /// <returns>
        /// The RavenQuestionnaire.Core.Views.Question.CompleteQuestionView.
        /// </returns>
        public CompleteQuestionView CreateQuestion(CompleteQuestionnaireStoreDocument doc, ICompleteQuestion question)
        {
            return new CompleteQuestionView(doc, question);
        }

        #endregion
    }
}