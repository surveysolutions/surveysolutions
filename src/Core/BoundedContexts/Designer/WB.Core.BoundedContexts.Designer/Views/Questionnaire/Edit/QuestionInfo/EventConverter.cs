using System;
using Main.Core.Entities;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo
{
    internal static class EventConverter
    {
        public static QuestionData QRBarcodeQuestionUpdatedToQuestionData(IPublishedEvent<QRBarcodeQuestionUpdated> evnt)
        {
            QRBarcodeQuestionUpdated e = evnt.Payload;
            var data = new QuestionData(
                e.QuestionId,
                QuestionType.QRBarcode,
                e.QuestionScope,
                e.Title,
                e.VariableName,
                e.VariableLabel,
                e.EnablementCondition,
                e.HideIfDisabled,
                Order.AZ,
                false,
                false,
                e.Instructions,
                e.Properties,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                e.ValidationConditions,
                null,
                false);
            return data;
        }

        public static QuestionData QRBarcodeQuestionClonedToQuestionData(IPublishedEvent<QRBarcodeQuestionCloned> evnt)
        {
            QRBarcodeQuestionCloned e = evnt.Payload;
            var data = new QuestionData(
                e.QuestionId,
                QuestionType.QRBarcode,
                e.QuestionScope,
                e.Title,
                e.VariableName,
                e.VariableLabel,
                e.EnablementCondition,
                e.HideIfDisabled,
                Order.AZ,
                false,
                false,
                e.Instructions,
                e.Properties,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                e.ValidationConditions,
                null,
                false);
            return data;
        }

        public static QuestionData MultimediaQuestionUpdatedToQuestionData(IPublishedEvent<MultimediaQuestionUpdated> evnt)
        {
            MultimediaQuestionUpdated e = evnt.Payload;
            var data = new QuestionData(
                e.QuestionId,
                QuestionType.Multimedia,
                e.QuestionScope,
                e.Title,
                e.VariableName,
                e.VariableLabel,
                e.EnablementCondition,
                e.HideIfDisabled,
                Order.AZ,
                false,
                false,
                e.Instructions,
                e.Properties,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                e.ValidationConditions,
                null,
                false);
            return data;
        }

        public static QuestionData TextListQuestionClonedToQuestionData(IPublishedEvent<TextListQuestionCloned> evnt)
        {
            TextListQuestionCloned e = evnt.Payload;
            var data = new QuestionData(
                e.PublicKey,
                QuestionType.TextList,
                e.QuestionScope,
                e.QuestionText,
                e.StataExportCaption,
                e.VariableLabel,
                e.ConditionExpression,
                e.HideIfDisabled,
                Order.AZ,
                false,
                false,
                e.Instructions,
                e.Properties,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                e.MaxAnswerCount,
                null,
                null,
                null,
                e.ValidationConditions,
                null,
                false);
            return data;
        }

        public static QuestionData TextListQuestionChangedToQuestionData(IPublishedEvent<TextListQuestionChanged> evnt)
        {
            TextListQuestionChanged e = evnt.Payload;
            var data = new QuestionData(
                e.PublicKey,
                QuestionType.TextList,
                e.QuestionScope,
                e.QuestionText,
                e.StataExportCaption,
                e.VariableLabel,
                e.ConditionExpression,
                e.HideIfDisabled,
                Order.AZ,
                false,
                false,
                e.Instructions,
                e.Properties,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                e.MaxAnswerCount,
                null,
                null,
                null,
                e.ValidationConditions,
                null,
                false);
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
                e.HideIfDisabled,
                e.AnswerOrder,
                e.Featured,
                e.Capital,
                e.Instructions,
                e.Properties,
                e.Mask,
                GetValidAnswersCollection(e.Answers),
                e.LinkedToQuestionId,
                e.LinkedToRosterId,
                e.IsInteger,
                null,
                e.AreAnswersOrdered,
                e.MaxAllowedAnswers,
                null,
                e.IsFilteredCombobox,
                e.CascadeFromQuestionId,
                null,
                e.ValidationConditions,
                null,
                e.IsTimestamp);
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
                e.HideIfDisabled,
                e.AnswerOrder,
                e.Featured,
                e.Capital,
                e.Instructions,
                e.Properties,
                e.Mask,
                 GetValidAnswersCollection(e.Answers),
                e.LinkedToQuestionId,
                e.LinkedToRosterId,
                e.IsInteger,
                e.CountOfDecimalPlaces,
                e.AreAnswersOrdered,
                e.MaxAllowedAnswers,
                e.MaxAnswerCount,
                e.IsFilteredCombobox,
                e.CascadeFromQuestionId,
                e.YesNoView,
                e.ValidationConditions,e.LinkedFilterExpression,
                e.IsTimestamp);
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
                e.HideIfDisabled,
                e.AnswerOrder,
                e.Featured,
                e.Capital,
                e.Instructions,
                e.Properties,
                e.Mask,
                GetValidAnswersCollection(e.Answers),
                e.LinkedToQuestionId,
                e.LinkedToRosterId,
                e.IsInteger,
                null,
                e.AreAnswersOrdered,
                e.MaxAllowedAnswers,
                null,
                e.IsFilteredCombobox,
                e.CascadeFromQuestionId,
                e.YesNoView,
                e.ValidationConditions, e.LinkedFilterExpression,
                e.IsTimestamp);
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
                e.HideIfDisabled,
                Order.AZ,
                e.Featured,
                e.Capital,
                e.Instructions,
                e.Properties,
                null,
                null,
                null,
                null,
                e.IsInteger,
                e.CountOfDecimalPlaces,
                null,
                null,
                null,
                null,
                null,
                null,
                e.ValidationConditions,
                null,
                false);
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
                e.HideIfDisabled,
                Order.AZ,
                e.Featured,
                e.Capital,
                e.Instructions,
                e.Properties,
                null,
                null,
                null,
                null,
                e.IsInteger,
                e.CountOfDecimalPlaces,
                null,
                null,
                null,
                null,
                null,
                null,
                e.ValidationConditions,
                null,
                false);
            data.ValidationConditions = evnt.Payload.ValidationConditions;
            return data;
        }

        #region for very-very old and invalid events
        private static QuestionType GetQuestionType(QuestionType type)
        {
            if (type == QuestionType.AutoPropagate)
                return QuestionType.Numeric;

            if (type == QuestionType.YesNo)
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