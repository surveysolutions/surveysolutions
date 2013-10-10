using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Entities;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.Entities.SubEntities.Question;

namespace Main.Core.AbstractFactories
{
    public class QuestionFactory : IQuestionFactory
    {
        public AbstractQuestion CreateQuestion(QuestionData data)
        {
            AbstractQuestion q = CreateQuestion(data.questionType, data.publicKey);

            UpdateQuestion(
                q,
                data.questionType,
                data.questionScope,
                data.questionText,
                data.stataExportCaption,
                data.conditionExpression,
                data.validationExpression,
                data.validationMessage,
                data.answerOrder,
                data.featured,
                data.mandatory,
                data.capital,
                data.instructions,
                data.triggers,
                data.maxValue,
                data.linkedToQuestionId,
                data.isInteger);

            UpdateAnswerList(data.answers, q, data.linkedToQuestionId);

            return q;
        }

        private AbstractQuestion CreateQuestion(QuestionType questionType, Guid publicKey)
        {
            AbstractQuestion q = CreateQuestion(questionType);

            q.PublicKey = publicKey;

            return q;
        }

        private AbstractQuestion CreateQuestion(QuestionType type)
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

        private static void UpdateAnswerList(IEnumerable<IAnswer> answers, IQuestion question, Guid? linkedToQuestionId)
        {
            if (question.Answers != null)
            {
                question.Answers.Clear();
            }

            if (!linkedToQuestionId.HasValue && answers != null && answers.Any())
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
            int maxValue,
            Guid? linkedToQuestionId,
            bool? isInteger)
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
            question.LinkedToQuestionId = linkedToQuestionId;

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

            var numericQuestion = question as INumericQuestion;
            if (numericQuestion != null)
            {
                numericQuestion.IsInteger = isInteger ?? false;
            }
        }
    }
}
