using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;

namespace WB.Core.SharedKernels.Questionnaire.Documents
{
    public static class ICompositeExtensions
    {
        public static string GetTitle(this IComposite entity)
        {
            return  (entity as IQuestion)?.QuestionText ?? (entity as IGroup)?.Title ?? (entity as IStaticText)?.Text;
        }
    }
}