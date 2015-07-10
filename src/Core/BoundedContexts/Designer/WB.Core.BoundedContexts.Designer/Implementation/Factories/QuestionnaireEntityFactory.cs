using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;

namespace WB.Core.BoundedContexts.Designer.Implementation.Factories
{
    internal class QuestionnaireEntityFactory : IQuestionnaireEntityFactory
    {
        public IStaticText CreateStaticText(Guid entityId, string text)
        {
            return new StaticText(publicKey: entityId, text: System.Web.HttpUtility.HtmlDecode(text));
        }

        public IQuestion CreateQuestion(QuestionData data)
        {
            AbstractQuestion q = CreateQuestion(data.QuestionType, data.PublicKey);

            UpdateQuestion(
                q,
                data.QuestionType,
                data.QuestionScope,
                data.QuestionText,
                data.StataExportCaption,
                data.VariableLabel,
                data.ConditionExpression,
                data.ValidationExpression,
                data.ValidationMessage,
                data.AnswerOrder,
                data.Featured,
                data.Mandatory,
                data.Capital,
                data.Instructions,
                data.Mask,
                data.LinkedToQuestionId,
                data.QuestionType == QuestionType.AutoPropagate ? true : data.IsInteger,
                data.CountOfDecimalPlaces,
                data.AreAnswersOrdered,
                data.MaxAllowedAnswers,
                data.MaxAnswerCount,
                data.IsFilteredCombobox,
                data.CascadeFromQuestionId);

            UpdateAnswerList(data.Answers, q, data.LinkedToQuestionId);

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
                    return new NumericQuestion()
                    {
                        IsInteger = true
                    };

                case QuestionType.GpsCoordinates:
                    return new GpsCoordinateQuestion();

                case QuestionType.TextList:
                    return new TextListQuestion();

                case QuestionType.QRBarcode:
                    return new QRBarcodeQuestion();

                case QuestionType.Multimedia:
                    return new MultimediaQuestion();

                default:
                    throw new NotSupportedException(string.Format("Question type is not supported: {0}", type));
            }
        }

        private static void UpdateAnswerList(IEnumerable<Answer> answers, IQuestion question, Guid? linkedToQuestionId)
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
            string variableLabel,
            string conditionExpression,
            string validationExpression,
            string validationMessage,
            Order answerOrder,
            bool featured,
            bool mandatory,
            bool capital,
            string instructions,
            string mask,
            Guid? linkedToQuestionId,
            bool? isInteger,
            int? countOfDecimalPlaces,
            bool? areAnswersOrdered,
            int? maxAllowedAnswers,
            int? masAnswerCount,
            bool? isFilteredCombobox,
            Guid? cascadeFromQuestionId)
        {
            question.QuestionType = questionType;
            question.QuestionScope = questionScope;
            question.QuestionText = System.Web.HttpUtility.HtmlDecode(questionText);
            question.StataExportCaption = stataExportCaption;
            question.VariableLabel = variableLabel;
            question.ConditionExpression = conditionExpression;
            question.ValidationExpression = validationExpression;
            question.ValidationMessage = validationMessage;
            question.AnswerOrder = answerOrder;
            question.Featured = featured;
            question.Mandatory = mandatory;
            question.Instructions = instructions;
            question.Capital = capital;
            question.LinkedToQuestionId = linkedToQuestionId;
            question.IsFilteredCombobox = isFilteredCombobox;
            question.CascadeFromQuestionId = cascadeFromQuestionId;

            var numericQuestion = question as INumericQuestion;
            if (numericQuestion != null)
            {
                numericQuestion.IsInteger = isInteger ?? false;
                numericQuestion.CountOfDecimalPlaces = countOfDecimalPlaces;
                numericQuestion.QuestionType = QuestionType.Numeric;
            }

            var multioptionQuestion = question as IMultyOptionsQuestion;
            if (multioptionQuestion != null)
            {
                multioptionQuestion.AreAnswersOrdered = areAnswersOrdered ?? false;
                multioptionQuestion.MaxAllowedAnswers = maxAllowedAnswers;
            }

            var listQuestion = question as ITextListQuestion;
            if (listQuestion != null)
            {
                listQuestion.MaxAnswerCount = masAnswerCount;
            }
            var textQuestion = question as TextQuestion;
            if (textQuestion != null)
            {
                textQuestion.Mask = mask;
            }
        }
    }
}
