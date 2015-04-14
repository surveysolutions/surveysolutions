using System;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public interface IInterviewItemViewModel
    {
        bool IsDisabled { get; set; }
    }

    public abstract class AbstractQuestionViewModel : IInterviewItemViewModel
    {
        public bool IsDisabled { get; set; }
        public Guid Id { get; set; }
        public decimal[] RosterVector { get; set; }
        public string Title { get; set; }
    }
}
