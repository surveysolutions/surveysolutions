namespace Main.Core.AbstractFactories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;
    using Main.Core.Entities.SubEntities.Complete.Question;
    using Main.Core.Entities.SubEntities.Question;
    using Main.Core.Utility.OrderStrategy;

    public class CompleteQuestionFactory : ICompleteQuestionFactory
    {
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

        public AbstractQuestion CreateQuestion(Guid publicKey, QuestionType questionType,
            QuestionScope questionScope, string questionText, string stataExportCaption,
            string conditionExpression, string validationExpression, string validationMessage,
            Order answerOrder, bool featured, bool mandatory, bool capital, string instructions,
            List<Guid> triggers, int maxValue, Answer[] answers)
        {
            AbstractQuestion q = CreateQuestion(questionType);

            q.PublicKey = publicKey;

            UpdateQuestion(
                q,
                questionType,
                questionScope,
                questionText,
                stataExportCaption,
                conditionExpression,
                validationExpression,
                validationMessage,
                answerOrder,
                featured,
                mandatory,
                capital,
                instructions,
                triggers,
                maxValue);

            UpdateAnswerList(answers, q);

            return q;
        }

        public IQuestion CreateQuestionFromExistingUsingSpecifiedData(IQuestion question, QuestionType questionType,
            QuestionScope questionScope, string questionText, string stataExportCaption,
            string conditionExpression, string validationExpression, string validationMessage,
            Order answerOrder, bool featured, bool mandatory, bool capital, string instructions,
            List<Guid> triggers, int maxValue, Answer[] answers)
        {
            AbstractQuestion q = CreateQuestion(questionType);

            q.PublicKey = question.PublicKey;

            UpdateQuestion(
                q,
                questionType,
                questionScope,
                questionText,
                stataExportCaption,
                conditionExpression,
                validationExpression,
                validationMessage,
                answerOrder,
                featured,
                mandatory,
                capital,
                instructions,
                triggers,
                maxValue);

            UpdateAnswerList(answers, q);

            return q;
        }

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
    }
}