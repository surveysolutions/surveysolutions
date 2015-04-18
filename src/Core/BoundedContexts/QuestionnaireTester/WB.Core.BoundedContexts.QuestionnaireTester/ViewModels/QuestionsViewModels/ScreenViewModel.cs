using System;
using Cirrious.MvvmCross.ViewModels;

using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public interface IInterviewItemViewModel
    {
        bool IsDisabled { get; set; }
    }

      public abstract class BaseInterviewItemViewModel : MvxViewModel
      {
          public abstract void Init(
              Identity identity,
              InterviewModel interviewModel,
              QuestionnaireModel questionnaireModel);
      }

    public abstract class AbstractQuestionViewModel : MvxViewModel, IInterviewItemViewModel
    {
        public bool IsDisabled { get; set; }
        public Guid Id { get; set; }
        public decimal[] RosterVector { get; set; }
        public string Title { get; set; }
    }
}
