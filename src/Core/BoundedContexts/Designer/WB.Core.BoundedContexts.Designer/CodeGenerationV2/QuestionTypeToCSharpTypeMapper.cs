using System;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Portable;
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