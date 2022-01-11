using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport
{
    public static class PreloadingExtensions
    {
        public const string MissingNumericQuestionValue = "-999999999";
        public const string MissingStringQuestionValue = "##N/A##";
        
        public static AssignmentAnswers ToAssignmentAnswers(this PreloadingCompositeValue compositeValue, IQuestionnaire questionnaire)
        {
            var questionId = questionnaire.GetQuestionIdByVariable(compositeValue.VariableOrCodeOrPropertyName);
            if (questionId.HasValue)
            {
                var answerType = questionnaire.GetAnswerType(questionId.Value);
                switch (answerType)
                {
                    case AnswerType.DecimalAndStringArray:
                        return ToAssignmentTextListAnswer(compositeValue);
                    case AnswerType.GpsData:
                        return ToAssignmentGpsAnswer(compositeValue);
                    case AnswerType.OptionCodeArray:
                    case AnswerType.YesNoArray:
                        {
                            compositeValue.Values.ForEach(x => x.VariableOrCodeOrPropertyName = x.VariableOrCodeOrPropertyName.Replace("n", "-"));
                            return ToAssignmentCategoricalMultiAnswer(compositeValue);
                        }
                    case AnswerType.RosterVectorArray:
                    {
                        return new AssignmentAnswers()
                        {
                            VariableName = compositeValue.VariableOrCodeOrPropertyName, 
                            Values = compositeValue.Values.Select(x => new AssignmentAnswer()
                            {
                                Value = x.Value,
                                Column = x.Column
                            } ).ToArray(),
                        };
                    }
                }
            }

            return null;
        }

        private static AssignmentAnswer ToGpsPropertyAnswer(this PreloadingValue answer)
            => answer.VariableOrCodeOrPropertyName == nameof(GeoPosition.Timestamp).ToLower()
                ? ToAssignmentDateTimeAnswer(answer)
                : ToAssignmentDoubleAnswer(answer);

        public static AssignmentRosterInstanceCode ToAssignmentRosterInstanceCode(this PreloadingValue answer)
        {
            int? intValue = null;
            if (int.TryParse(answer.Value, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat,
                out var intNumericValue))
                intValue = intNumericValue;

            return new AssignmentRosterInstanceCode
            {
                Code = intValue,
                VariableName = answer.VariableOrCodeOrPropertyName,
                Column = answer.Column,
                Value = answer.Value,
            };
        }

        public static AssignmentAnswer ToAssignmentAnswer(this PreloadingValue answer, IQuestionnaire questionnaire)
        {
            var questionId = questionnaire.GetQuestionIdByVariable(answer.VariableOrCodeOrPropertyName);
            if (questionId.HasValue)
            {
                var answerType = questionnaire.GetAnswerType(questionId.Value);

                switch (answerType)
                {
                    case AnswerType.OptionCode:
                        return ToAssignmentCategoricalSingleAnswer(answer);
                    case AnswerType.Integer:
                        return ToAssignmentIntegerAnswer(answer);
                    case AnswerType.Decimal:
                        return ToAssignmentDoubleAnswer(answer);
                    case AnswerType.DateTime:
                        return ToAssignmentDateTimeAnswer(answer);
                    case AnswerType.String:
                    case AnswerType.DecimalAndStringArray:
                    default:
                        return ToAssignmentTextAnswer(answer);
                }
            }

            return null;
        }

        private static AssignmentGpsAnswer ToAssignmentGpsAnswer(this PreloadingCompositeValue compositeValue)
            => new AssignmentGpsAnswer
            {
                VariableName = compositeValue.VariableOrCodeOrPropertyName,
                Values = compositeValue.Values.Select(ToGpsPropertyAnswer).ToArray()
            };

        private static AssignmentMultiAnswer ToAssignmentTextListAnswer(this PreloadingCompositeValue compositeValue)
            => new AssignmentMultiAnswer
            {
                VariableName = compositeValue.VariableOrCodeOrPropertyName,
                Values = compositeValue.Values.Select(ToAssignmentTextAnswer).ToArray()
            };

        private static AssignmentMultiAnswer ToAssignmentCategoricalMultiAnswer(this PreloadingCompositeValue compositeValue)
            => new AssignmentMultiAnswer
            {
                VariableName = compositeValue.VariableOrCodeOrPropertyName,
                Values = compositeValue.Values.Select(ToAssignmentIntegerAnswer).ToArray()
            };

        private static AssignmentAnswer ToAssignmentDoubleAnswer(this PreloadingValue answer)
        {
            if (string.Compare(MissingNumericQuestionValue, answer.Value, StringComparison.InvariantCultureIgnoreCase) == 0)
                answer.Value = string.Empty;
            
            double? doubleValue = null;
            if (double.TryParse(answer.Value, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat,
                out var doubleNumericValue))
                doubleValue = doubleNumericValue;

            return new AssignmentDoubleAnswer
            {
                Answer = doubleValue,
                VariableName = answer.VariableOrCodeOrPropertyName,
                Column = answer.Column,
                Value = answer.Value,
            };
        }

        private static AssignmentAnswer ToAssignmentDateTimeAnswer(this PreloadingValue answer)
        {
            DateTime? dataTimeValue = null;
            if (DateTime.TryParse(answer.Value, null, DateTimeStyles.AdjustToUniversal, out var date))
                dataTimeValue = date;

            return new AssignmentDateTimeAnswer
            {
                Answer = dataTimeValue,
                VariableName = answer.VariableOrCodeOrPropertyName,
                Column = answer.Column,
                Value = answer.Value,
            };
        }

        private static AssignmentAnswer ToAssignmentCategoricalSingleAnswer(this PreloadingValue answer)
        {
            var integerAnswer = (AssignmentIntegerAnswer)ToAssignmentIntegerAnswer(answer);

            return new AssignmentCategoricalSingleAnswer
            {
                VariableName = integerAnswer.VariableName,
                Value = integerAnswer.Value,
                Column = integerAnswer.Column,
                OptionCode = integerAnswer.Answer
            };
        }

        private static AssignmentAnswer ToAssignmentTextAnswer(this PreloadingValue answer)
            => new AssignmentTextAnswer
            {
                VariableName = answer.VariableOrCodeOrPropertyName,
                Column = answer.Column,
                Value = string.Compare(MissingStringQuestionValue, answer.Value, StringComparison.InvariantCultureIgnoreCase) == 0 ? string.Empty :  answer.Value,
            };

        private static AssignmentAnswer ToAssignmentIntegerAnswer(this PreloadingValue answer)
        {
            if (string.Compare(MissingNumericQuestionValue, answer.Value, StringComparison.InvariantCultureIgnoreCase) == 0)
                answer.Value = string.Empty;
            
            int? intValue = null;
            if (int.TryParse(answer.Value, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat,
                out var intNumericValue))
                intValue = intNumericValue;

            return new AssignmentIntegerAnswer
            {
                Answer = intValue,
                VariableName = answer.VariableOrCodeOrPropertyName,
                Column = answer.Column,
                Value = answer.Value,
            };
        }

        public static AssignmentQuantity ToAssignmentQuantity(this PreloadingValue answer)
        {
            int? quantityValue = null;

            if (int.TryParse(answer.Value, out var quantity))
                quantityValue = quantity;

            return new AssignmentQuantity
            {
                Quantity = quantityValue,
                Column = answer.Column,
                Value = answer.Value
            };
        }

        public static AssignmentComments ToAssignmentComments(this PreloadingValue answer) =>
            new AssignmentComments
            {
                Column = answer.Column,
                Value = answer.Value
            };

        public static AssignmentRecordAudio ToAssignmentRecordAudio(this PreloadingValue preloadingWebMode)
        {
            var recordAudio = new AssignmentRecordAudio
            {
                Column = preloadingWebMode.Column,
                Value = preloadingWebMode.Value
            };

            if (int.TryParse(preloadingWebMode.Value, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out var intNumericValue))
                recordAudio.DoesNeedRecord = intNumericValue == 1;

            return recordAudio;
        }

        public static AssignmentWebMode ToAssignmentWebMode(this PreloadingValue preloadingWebMode)
        {
            var webMode = new AssignmentWebMode
            {
                Column = preloadingWebMode.Column,
                Value = preloadingWebMode.Value

            };

            if (int.TryParse(preloadingWebMode.Value, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out var intNumericValue))
                webMode.WebMode = intNumericValue == 1;

            return webMode;
        }

        public static AssignmentInterviewId ToAssignmentInterviewId(this PreloadingValue value)
            => new AssignmentInterviewId
            {
                Column = value.Column,
                Value = value.Value
            };
        
        public static AssignmentPassword ToAssignmentPassword(this PreloadingValue value)
            => new AssignmentPassword
            {
                Column = value.Column,
                Value = value.Value
            };
        
        public static AssignmentEmail ToAssignmentEmail(this PreloadingValue value)
            => new AssignmentEmail
            {
                Column = value.Column,
                Value = value.Value
            };

        public static AssignmentResponsible ToAssignmentResponsible(this PreloadingValue answer,
            IUserViewFactory userViewFactory, Dictionary<string, UserToVerify> usersCache)
        {
            var responsible = new AssignmentResponsible
            {
                Column = answer.Column,
                Value = answer.Value
            };

            var responsibleName = answer.Value;
            if (!string.IsNullOrWhiteSpace(responsibleName))
            {
                if (!usersCache.ContainsKey(responsibleName))
                    usersCache.Add(responsibleName,
                        userViewFactory.GetUsersByUserNames(new[] {responsibleName}).FirstOrDefault());

                responsible.Responsible = usersCache[responsibleName];
            }

            return responsible;
        }
    }
}
