using Moq;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;


namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.FilteredOptionsViewModelTests
{
    [NUnit.Framework.TestOf(typeof(FilteredOptionsViewModel))]
    public class FilteredOptionsViewModelTestContext
    {
        protected static FilteredOptionsViewModel CreateViewModel(IQuestionnaireStorage questionnaireRepository = null,
            IStatefulInterviewRepository interviewRepository = null,
            AnswerNotifier answerNotifier = null)
        {
            return new FilteredOptionsViewModel(
                questionnaireRepository ?? Mock.Of<IQuestionnaireStorage>(),
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                answerNotifier ?? Mock.Of<AnswerNotifier>());
        }
    }
}