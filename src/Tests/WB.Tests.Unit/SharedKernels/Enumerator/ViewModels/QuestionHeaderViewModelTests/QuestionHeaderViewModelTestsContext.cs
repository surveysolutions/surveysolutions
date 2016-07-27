﻿using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.QuestionHeaderViewModelTests
{
    [Subject(typeof(QuestionHeaderViewModel))]
    internal class QuestionHeaderViewModelTestsContext
    {
        public static QuestionHeaderViewModel CreateViewModel(IQuestionnaireStorage questionnaireRepository = null, 
            IStatefulInterviewRepository interviewRepository = null, 
            ILiteEventRegistry registry = null,
            IRosterTitleSubstitutionService rosterTitleSubstitutionService = null)
        {
            return new QuestionHeaderViewModel(
                questionnaireRepository ?? Mock.Of<IQuestionnaireStorage>(),
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                Create.ViewModel.DynamicTextViewModel(
                    interviewRepository: interviewRepository,
                    questionnaireRepository: questionnaireRepository,
                    rosterTitleSubstitutionService: rosterTitleSubstitutionService));
        }
    }
}