using WB.Core.SharedKernels.Questionnaire.Documents;

namespace Main.Core.Entities.SubEntities.Question
{
    public interface IMultyOptionsQuestion : IQuestion, ICategoricalQuestion
    {
        bool AreAnswersOrdered { get; set; }

        int? MaxAllowedAnswers { get; set; }

        bool YesNoView { get; set; }
    }
}