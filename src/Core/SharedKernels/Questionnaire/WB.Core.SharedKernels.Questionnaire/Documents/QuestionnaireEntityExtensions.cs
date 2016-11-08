using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
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

        public static string GetVariable(this IQuestionnaireEntity entity)
            => (entity as IQuestion)?.StataExportCaption
            ?? (entity as IGroup)?.VariableName
            ?? (entity as IVariable)?.Name;

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