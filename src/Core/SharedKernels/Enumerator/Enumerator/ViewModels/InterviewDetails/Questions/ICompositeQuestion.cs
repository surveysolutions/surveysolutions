using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public interface ICompositeQuestion
    {
        QuestionInstructionViewModel InstructionViewModel { get; }
        IQuestionStateViewModel QuestionState { get; }
    }
}