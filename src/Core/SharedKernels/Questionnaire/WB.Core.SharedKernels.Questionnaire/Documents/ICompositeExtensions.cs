using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;

namespace WB.Core.SharedKernels.Questionnaire.Documents
{
    public static class ICompositeExtensions
    {
        public static string GetTitle(this IComposite entity)
            => (entity as IQuestion)?.QuestionText
            ?? (entity as IStaticText)?.Text
            ?? (entity as IGroup)?.Title;
    }
}