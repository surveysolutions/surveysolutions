using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.SharedKernels.Questionnaire.Documents
{
    public static class QuestionnaireEntityExtensions
    {
        public static IQuestionnaireEntity GetPrevious(this IQuestionnaireEntity entity)
        {
            var parent = entity.GetParent();
            if (parent == null) return null;

            var indexInParent = parent.Children.IndexOf(entity as IComposite);
            if (indexInParent <= 0) return null;

            return parent.Children[indexInParent - 1];
        }

        public static string GetTitle(this IQuestionnaireEntity entity)
            => (entity as IQuestion)?.QuestionText
            ?? (entity as IStaticText)?.Text
            ?? (entity as IGroup)?.Title;

        public static IEnumerable<string> GetAllExpressions(this IQuestionnaireEntity entity)
        {
            var validations = (entity as IValidatable)?.ValidationConditions?.Select(validation => validation.Expression) ?? Enumerable.Empty<string>();
            var conditions = (entity as IConditional)?.ConditionExpression.ToEnumerable() ?? Enumerable.Empty<string>();

            return Enumerable.Concat(validations, conditions).Where(expression => expression != null);
        }

        public static IEnumerable<IQuestionnaireEntity> GetDescendants(this IGroup group)
            => group.Children.TreeToEnumerable(child => child.Children);

        public static string GetEnablingCondition(this IQuestionnaireEntity entity)
            => (entity as IConditional)?.ConditionExpression;

        public static IEnumerable<ValidationCondition> GetValidationConditions(this IQuestionnaireEntity entity)
            => (entity as IValidatable)?.ValidationConditions;

        public static string GetVariable(this IQuestionnaireEntity entity)
            => (entity as IQuestion)?.StataExportCaption
            ?? (entity as IGroup)?.VariableName
            ?? (entity as IVariable)?.Name;

        public static string GetQuestionType(this IComposite entity, QuestionnaireDocument questionnaire) 
            => (entity as IVariable)?.Type.ToString()
            ?? (entity is IGroup ? "Roster" : null)
            // use IQuestionTypeToCSharpTypeMapper for questions here 
            ?? (entity as IQuestion)?.GenerateQuestionTypeName(questionnaire).Replace("Tuple<decimal, string>[]", "TextList");

        public static string GenerateQuestionTypeName(this IQuestion question, QuestionnaireDocument questionnaire)
        {
            switch (question.QuestionType)
            {
                case QuestionType.Text:
                case QuestionType.QRBarcode:
                case QuestionType.Multimedia:
                case QuestionType.Area:
                    return "string";

                case QuestionType.AutoPropagate:
                    return "long?";

                case QuestionType.Numeric:
                    return (question as NumericQuestion).IsInteger ? "long?" : "double?";

                case QuestionType.MultyOption:
                    var multiOtion = question as MultyOptionsQuestion;
                    if (multiOtion != null && multiOtion.YesNoView)
                        return nameof(YesNoAnswers);

                    if (question.LinkedToQuestionId == null && question.LinkedToRosterId == null)
                        return "decimal[]";

                    if (question.LinkedToQuestionId.HasValue && questionnaire.Find<ITextListQuestion>(question.LinkedToQuestionId.Value) != null)
                    {
                        return "decimal[]";
                    }
                    return "decimal[][]";

                case QuestionType.DateTime:
                    return "DateTime?";

                case QuestionType.SingleOption:
                    if (question.LinkedToQuestionId == null && question.LinkedToRosterId == null) return "decimal?";

                    if (question.LinkedToQuestionId.HasValue && questionnaire.Find<ITextListQuestion>(question.LinkedToQuestionId.Value) != null)
                    {
                        return "decimal?";
                    }

                    return "decimal[]";
                case QuestionType.TextList:
                    return "Tuple<decimal, string>[]";

                case QuestionType.GpsCoordinates:
                    return "GeoLocation";

                default:
                    throw new ArgumentException("Unknown question type.");
            }
        }

        public static void SetVariable(this IQuestionnaireEntity entity, string variableName)
        {
            var question = entity as IQuestion;
            var variable = entity as IVariable;
            var group = entity as IGroup;

            if (question != null) question.StataExportCaption = variableName;
            if (variable != null) variable.Name = variableName;
            if (group != null) group.VariableName = variableName;
        }

        public static void SetTitle(this IQuestionnaireEntity entity, string title)
        {
            var question = entity as IQuestion;
            var staticText = entity as IStaticText;
            var group = entity as IGroup;

            if (question != null) question.QuestionText = title;
            if (staticText != null) staticText.Text = title;
            if (group != null) group.Title = title;
        }
    }
}