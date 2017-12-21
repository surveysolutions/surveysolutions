using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.CodeGenerationV2;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Designer.CodeGenerationV3.Impl
{
    public class QuestionTypeToCSharpTypeMapperV3 : QuestionTypeToCSharpTypeMapper, IQuestionTypeToCSharpTypeMapperV3
    {
        public override bool IsAnswerValueType(IQuestion question, ReadOnlyQuestionnaireDocument questionnaire)
        {
            switch (question.QuestionType)
            {
                case QuestionType.Numeric:
                case QuestionType.DateTime:
                    return true;
                case QuestionType.SingleOption:
                    if (question.LinkedToQuestionId == null && question.LinkedToRosterId == null) return true;
                    if (question.LinkedToQuestionId.HasValue && questionnaire.Find<ITextListQuestion>(question.LinkedToQuestionId.Value) != null)
                    {
                        return true;
                    }
                    return false;
                default: return false;
            }
        }

        public override string GetQuestionType(IQuestion question, ReadOnlyQuestionnaireDocument questionnaire)
        {
            switch (question.QuestionType)
            {
                case QuestionType.Numeric:
                    return ((question as NumericQuestion)?.IsInteger ?? false) ? "int" : "double";
                case QuestionType.DateTime:
                    return "DateTime";

                case QuestionType.SingleOption:
                    if (question.LinkedToQuestionId == null && question.LinkedToRosterId == null) return "int";

                    if (question.LinkedToQuestionId.HasValue && questionnaire.Find<ITextListQuestion>(question.LinkedToQuestionId.Value) != null)
                    {
                        return "int";
                    }

                    return typeof(RosterVector).Name;
                default:
                    return base.GetQuestionType(question, questionnaire);
            }
        }
    }
}