using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo
{
    internal static class EventConverter
    {
        public static QuestionData QRBarcodeQuestionAddedToQuestionData(IPublishedEvent<QRBarcodeQuestionAdded> evnt)
        {
            QRBarcodeQuestionAdded e = evnt.Payload;
            var data = new QuestionData(
                e.QuestionId,
                QuestionType.QRBarcode,
                QuestionScope.Interviewer,
                e.Title,
                e.VariableName,
                e.VariableLabel,
                e.EnablementCondition,
                null,
                null,
                Order.AZ,
                false,
                e.IsMandatory,
                false,
                e.Instructions,
                null,
                new List<Guid>(),
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);
            return data;
        }

        public static QuestionData QRBarcodeQuestionUpdatedToQuestionData(IPublishedEvent<QRBarcodeQuestionUpdated> evnt)
        {
            QRBarcodeQuestionUpdated e = evnt.Payload;
            var data = new QuestionData(
                e.QuestionId,
                QuestionType.QRBarcode,
                QuestionScope.Interviewer,
                e.Title,
                e.VariableName,
                e.VariableLabel,
                e.EnablementCondition,
                e.ValidationExpression,
                e.ValidationMessage,
                Order.AZ,
                false,
                e.IsMandatory,
                false,
                e.Instructions,
                null,
                new List<Guid>(),
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);
            return data;
        }

        public static QuestionData QRBarcodeQuestionClonedToQuestionData(IPublishedEvent<QRBarcodeQuestionCloned> evnt)
        {
            QRBarcodeQuestionCloned e = evnt.Payload;
            var data = new QuestionData(
                e.QuestionId,
                QuestionType.QRBarcode,
                QuestionScope.Interviewer,
                e.Title,
                e.VariableName,
                e.VariableLabel,
                e.EnablementCondition,
                null,
                null,
                Order.AZ,
                false,
                e.IsMandatory,
                false,
                e.Instructions,
                null,
                new List<Guid>(),
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);
            return data;
        }

        public static QuestionData MultimediaQuestionUpdatedToQuestionData(IPublishedEvent<MultimediaQuestionUpdated> evnt)
        {
            MultimediaQuestionUpdated e = evnt.Payload;
            var data = new QuestionData(
                e.QuestionId,
                QuestionType.Multimedia,
                QuestionScope.Interviewer,
                e.Title,
                e.VariableName,
                e.VariableLabel,
                e.EnablementCondition,
                null,
                null,
                Order.AZ,
                false,
                e.IsMandatory,
                false,
                e.Instructions,
                null,
                new List<Guid>(),
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);
            return data;
        }

        public static QuestionData TextListQuestionAddedToQuestionData(IPublishedEvent<TextListQuestionAdded> evnt)
        {
            TextListQuestionAdded e = evnt.Payload;
            var data = new QuestionData(
                e.PublicKey,
                QuestionType.TextList,
                QuestionScope.Interviewer,
                e.QuestionText,
                e.StataExportCaption,
                e.VariableLabel,
                e.ConditionExpression,
                null,
                null,
                Order.AZ,
                false,
                e.Mandatory,
                false,
                e.Instructions,
                null,
                new List<Guid>(),
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                e.MaxAnswerCount,
                null,
                null);
            return data;
        }

        public static QuestionData TextListQuestionClonedToQuestionData(IPublishedEvent<TextListQuestionCloned> evnt)
        {
            TextListQuestionCloned e = evnt.Payload;
            var data = new QuestionData(
                e.PublicKey,
                QuestionType.TextList,
                QuestionScope.Interviewer,
                e.QuestionText,
                e.StataExportCaption,
                e.VariableLabel,
                e.ConditionExpression,
                null,
                null,
                Order.AZ,
                false,
                e.Mandatory,
                false,
                e.Instructions,
                null,
                new List<Guid>(),
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                e.MaxAnswerCount,
                null,
                null);
            return data;
        }

        public static QuestionData TextListQuestionChangedToQuestionData(IPublishedEvent<TextListQuestionChanged> evnt)
        {
            TextListQuestionChanged e = evnt.Payload;
            var data = new QuestionData(
                e.PublicKey,
                QuestionType.TextList,
                QuestionScope.Interviewer,
                e.QuestionText,
                e.StataExportCaption,
                e.VariableLabel,
                e.ConditionExpression,
                null,
                null,
                Order.AZ,
                false,
                e.Mandatory,
                false,
                e.Instructions,
                null,
                new List<Guid>(),
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                e.MaxAnswerCount,
                null,
                null);
            return data;
        }

        public static QuestionData NewQuestionAddedToQuestionData(IPublishedEvent<NewQuestionAdded> evnt)
        {
            FullQuestionDataEvent e = evnt.Payload;
            var data = new QuestionData(
                e.PublicKey,
               GetQuestionType(e.QuestionType),
                e.QuestionScope,
                e.QuestionText,
                e.StataExportCaption,
                e.VariableLabel,
                e.ConditionExpression,
                e.ValidationExpression,
                e.ValidationMessage,
                e.AnswerOrder,
                e.Featured,
                e.Mandatory,
                e.Capital,
                e.Instructions,
                e.Mask,
                e.Triggers,
                null,
                 GetValidAnswersCollection(e.Answers),
                e.LinkedToQuestionId,
                e.IsInteger,
                null,
                e.AreAnswersOrdered,
                e.MaxAllowedAnswers,
                null,
                e.IsFilteredCombobox,
                e.CascadeFromQuestionId);
            return data;
        }

        public static QuestionData QuestionClonedToQuestionData(IPublishedEvent<QuestionCloned> evnt)
        {
            QuestionCloned e = evnt.Payload;
            var data = new QuestionData(
                e.PublicKey,
                GetQuestionType(e.QuestionType),
                e.QuestionScope,
                e.QuestionText,
                e.StataExportCaption,
                e.VariableLabel,
                e.ConditionExpression,
                e.ValidationExpression,
                e.ValidationMessage,
                e.AnswerOrder,
                e.Featured,
                e.Mandatory,
                e.Capital,
                e.Instructions,
                e.Mask,
                e.Triggers,
                evnt.Payload.MaxValue,
                 GetValidAnswersCollection(e.Answers),
                e.LinkedToQuestionId,
                e.IsInteger,
                e.CountOfDecimalPlaces,
                e.AreAnswersOrdered,
                e.MaxAllowedAnswers,
                e.MaxAnswerCount,
                e.IsFilteredCombobox,
                e.CascadeFromQuestionId);
            return data;
        }

        public static QuestionData QuestionChangedToQuestionData(IPublishedEvent<QuestionChanged> evnt)
        {
            QuestionChanged e = evnt.Payload;
            var data = new QuestionData(
                e.PublicKey,
                GetQuestionType(e.QuestionType),
                e.QuestionScope,
                e.QuestionText,
                e.StataExportCaption,
                e.VariableLabel,
                e.ConditionExpression,
                e.ValidationExpression,
                e.ValidationMessage,
                e.AnswerOrder,
                e.Featured,
                e.Mandatory,
                e.Capital,
                e.Instructions,
                e.Mask,
                e.Triggers,
                null,
                GetValidAnswersCollection(e.Answers),
                e.LinkedToQuestionId,
                e.IsInteger,
                null,
                e.AreAnswersOrdered,
                e.MaxAllowedAnswers,
                null,
                e.IsFilteredCombobox,
                e.CascadeFromQuestionId);
            return data;
        }

        public static QuestionData NumericQuestionAddedToQuestionData(IPublishedEvent<NumericQuestionAdded> evnt)
        {
            NumericQuestionAdded e = evnt.Payload;
            var data = new QuestionData(
                e.PublicKey,
                QuestionType.Numeric,
                e.QuestionScope,
                e.QuestionText,
                e.StataExportCaption,
                e.VariableLabel,
                e.ConditionExpression,
                e.ValidationExpression,
                e.ValidationMessage,
                Order.AZ,
                e.Featured,
                e.Mandatory,
                e.Capital,
                e.Instructions,
                null,
                e.Triggers,
                e.MaxAllowedValue,
                null,
                null,
                e.IsInteger,
                e.CountOfDecimalPlaces,
                null,
                null,
                null,
                null,
                null);
            return data;
        }

        public static QuestionData NumericQuestionClonedToQuestionData(IPublishedEvent<NumericQuestionCloned> evnt)
        {
            NumericQuestionCloned e = evnt.Payload;
            var data = new QuestionData(
                e.PublicKey,
                QuestionType.Numeric,
                e.QuestionScope,
                e.QuestionText,
                e.StataExportCaption,
                e.VariableLabel,
                e.ConditionExpression,
                e.ValidationExpression,
                e.ValidationMessage,
                Order.AZ,
                e.Featured,
                e.Mandatory,
                e.Capital,
                e.Instructions,
                null,
                e.Triggers,
                e.MaxAllowedValue,
                null,
                null,
                e.IsInteger,
                e.CountOfDecimalPlaces,
                null,
                null,
                null,
                null,
                null);
            return data;
        }

        public static QuestionData NumericQuestionChangedToQuestionData(IPublishedEvent<NumericQuestionChanged> evnt)
        {
            NumericQuestionChanged e = evnt.Payload;
            var data = new QuestionData(
                e.PublicKey,
                QuestionType.Numeric,
                e.QuestionScope,
                e.QuestionText,
                e.StataExportCaption,
                e.VariableLabel,
                e.ConditionExpression,
                e.ValidationExpression,
                e.ValidationMessage,
                Order.AZ,
                e.Featured,
                e.Mandatory,
                e.Capital,
                e.Instructions,
                null,
                e.Triggers,
                e.MaxAllowedValue,
                null,
                null,
                e.IsInteger,
                e.CountOfDecimalPlaces,
                null,
                null,
                null,
                null,
                null);
            return data;
        }

        #region for very-very old and invalid events
        private static QuestionType GetQuestionType(QuestionType type)
        {
            if (type == QuestionType.AutoPropagate)
                return QuestionType.Numeric;

            if (type == QuestionType.YesNo || type == QuestionType.DropDownList)
                return QuestionType.SingleOption;

            return type;
        }

        internal static Answer[] GetValidAnswersCollection(Answer[] answers)
        {
            if (answers == null)
                return null;

            foreach (var answer in answers)
            {
                if (string.IsNullOrWhiteSpace(answer.AnswerValue))
                {
                    answer.AnswerValue = (new Random().NextDouble() * 100).ToString("0.00");
                }
                if (string.IsNullOrWhiteSpace(answer.AnswerText))
                {
                    answer.AnswerText = "Option " + answer.AnswerValue;
                }
            }
            return answers;
        } 
        #endregion
    }
}