using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.SharedKernels.QuestionnaireEntities;


namespace WB.Core.BoundedContexts.Designer.Implementation.Factories
{
    internal class QuestionnaireEntityFactory : IQuestionnaireEntityFactory
    {
        public IStaticText CreateStaticText(Guid entityId, string text, string attachmentName, 
            string enablementCondition, bool hideIfDisabled, IList<ValidationCondition> validationConditions)
        {
            return new StaticText(publicKey: entityId, 
                text: System.Web.HttpUtility.HtmlDecode(text), 
                enablementCondition:enablementCondition,
                hideIfDisabled:hideIfDisabled,
                validationConditions : validationConditions,
                attachmentName: attachmentName);
        }

        public IQuestion CreateQuestion(QuestionData data)
        {
            AbstractQuestion question = CreateQuestion(data.QuestionType, data.PublicKey);

            question.QuestionType = data.QuestionType;
            question.QuestionScope = data.QuestionScope;
            question.QuestionText = System.Web.HttpUtility.HtmlDecode(data.QuestionText);
            question.StataExportCaption = data.StataExportCaption;
            question.VariableLabel = data.VariableLabel;
            question.ConditionExpression = data.ConditionExpression;
            question.HideIfDisabled = data.HideIfDisabled;
            question.ValidationExpression = data.ValidationExpression;
            question.ValidationMessage = data.ValidationMessage;
            question.AnswerOrder = data.AnswerOrder;
            question.Featured = data.Featured;
            question.Instructions = data.Instructions;
            question.Properties = data.Properties ?? new QuestionProperties(false, false);
            question.Capital = data.Capital;
            question.LinkedToQuestionId = data.LinkedToQuestionId;
            question.LinkedToRosterId = data.LinkedToRosterId;
            question.LinkedFilterExpression = data.LinkedFilterExpression;
            question.IsFilteredCombobox = data.IsFilteredCombobox;
            question.CascadeFromQuestionId = data.CascadeFromQuestionId;
            question.ValidationConditions = data.ValidationConditions;

            var numericQuestion = question as INumericQuestion;
            if (numericQuestion != null)
            {
                numericQuestion.IsInteger = data.QuestionType == QuestionType.AutoPropagate ? true : data.IsInteger ?? false;
                numericQuestion.CountOfDecimalPlaces = data.CountOfDecimalPlaces;
                numericQuestion.QuestionType = QuestionType.Numeric;
                numericQuestion.UseFormatting = question.Properties?.UseFormatting ?? false;
            }

            var multioptionQuestion = question as IMultyOptionsQuestion;
            if (multioptionQuestion != null)
            {
                multioptionQuestion.AreAnswersOrdered = data.AreAnswersOrdered ?? false;
                multioptionQuestion.MaxAllowedAnswers = data.MaxAllowedAnswers;
                multioptionQuestion.YesNoView = data.YesNoView ?? false;
            }

            var listQuestion = question as ITextListQuestion;
            if (listQuestion != null)
            {
                listQuestion.MaxAnswerCount = data.MaxAnswerCount;
            }
            var textQuestion = question as TextQuestion;
            if (textQuestion != null)
            {
                textQuestion.Mask = data.Mask;
            }

            var dateTimeQuestion = question as DateTimeQuestion;
            if (dateTimeQuestion != null)
            {
                dateTimeQuestion.IsTimestamp = data.IsTimestamp;
            }

            UpdateAnswerList(data.Answers, question, data.LinkedToQuestionId);

            return question;
        }

        public IVariable CreateVariable(QuestionnaireVariableEvent variableEvent)
        {
            var variable = new Variable(variableEvent.EntityId, variableEvent.VariableData);
            return variable;
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
    }
}
