using System;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.ExpressionStorage;
using WB.Core.SharedKernels.DataCollection.Portable;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.CodeGenerationV2
{
    public class QuestionTypeToCSharpTypeMapper : IQuestionTypeToCSharpTypeMapper
    {
        public string GetQuestionType(IQuestion question, ReadOnlyQuestionnaireDocument questionnaire)
        {
            switch (question.QuestionType)
            {
                case QuestionType.Multimedia:
                case QuestionType.QRBarcode:
                case QuestionType.Text:
                case QuestionType.Area:
                    return "string";
                case QuestionType.Numeric:
                    return ((question as NumericQuestion)?.IsInteger ?? false) ? "int?" : "double?";
                case QuestionType.MultyOption:
                    var multiOtion = question as MultyOptionsQuestion;
                    if (multiOtion != null && multiOtion.YesNoView)
                        return typeof(YesNoAndAnswersMissings).Name;

                    if (question.LinkedToQuestionId == null && question.LinkedToRosterId == null)
                        return "int[]";

                    if (question.LinkedToQuestionId.HasValue && questionnaire.Find<ITextListQuestion>(question.LinkedToQuestionId.Value) != null)
                    {
                        return "int[]";
                    }
                    return $"{typeof(RosterVector).Name}[]";

                case QuestionType.DateTime:
                    return "DateTime?";

                case QuestionType.SingleOption:
                    if (question.LinkedToQuestionId == null && question.LinkedToRosterId == null) return "int?";

                    if (question.LinkedToQuestionId.HasValue && questionnaire.Find<ITextListQuestion>(question.LinkedToQuestionId.Value) != null)
                    {
                        return "int?";
                    }

                    return typeof(RosterVector).Name;
                case QuestionType.TextList:
                    return $"{typeof(TextListAnswerRow).Name}[]";

                case QuestionType.GpsCoordinates:
                    return typeof(GeoLocation).Name;

                case QuestionType.Audio:
                    return typeof(AudioAnswerForConditions).Name;
                default:
                    throw new ArgumentException("Unknown question type.");
            }
        }

        public string GetQuestionMethodName(string questionModelTypeName, int targetVersion)
        {
            if (targetVersion < 22)
                return $"GetAnswer<{questionModelTypeName}>";
            switch (questionModelTypeName)
            {
                case "AudioAnswerForConditions": return nameof(IInterviewStateForExpressions.GetAudioAnswer);
                case "DateTime?": return nameof(IInterviewStateForExpressions.GetDateTimeAnswer);
                case "TextListAnswerRow[]": return nameof(IInterviewStateForExpressions.GetTextListAnswer);
                case "GeoLocation": return nameof(IInterviewStateForExpressions.GetGeoLocationAnswer);
                case "RosterVector[]": return nameof(IInterviewStateForExpressions.GetRosterVectorArrayAnswer);
                case "RosterVector": return nameof(IInterviewStateForExpressions.GetRosterVectorAnswer);
                case "YesNoAndAnswersMissings": return nameof(IInterviewStateForExpressions.GetYesNoAnswer);
                case "int[]": return nameof(IInterviewStateForExpressions.GetIntArrayAnswer);
                case "double?": return nameof(IInterviewStateForExpressions.GetDoubleAnswer);
                case "int?": return nameof(IInterviewStateForExpressions.GetIntAnswer);
                case "string": return nameof(IInterviewStateForExpressions.GetStringAnswer);
                default:
                    throw new ArgumentOutOfRangeException(nameof(questionModelTypeName), questionModelTypeName, $"unknown question type {questionModelTypeName}");
            }
        }

        public string GetVariableType(VariableType variableType)
        {
            switch (variableType)
            {
                case VariableType.LongInteger:
                    return "long?";
                case VariableType.Double:
                    return "double?";
                case VariableType.Boolean:
                    return "bool?";
                case VariableType.DateTime:
                    return "DateTime?";
                case VariableType.String:
                    return "string";
                default:
                    throw new ArgumentOutOfRangeException(nameof(variableType), variableType, $"unknown variable type {variableType}");
            }
        }
    }
}