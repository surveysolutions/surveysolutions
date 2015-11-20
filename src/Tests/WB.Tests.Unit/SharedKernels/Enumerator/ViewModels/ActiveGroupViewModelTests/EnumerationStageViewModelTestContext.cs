using System;
using Moq;
using MvvmCross.Plugins.Messenger;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.SurveySolutions.Services;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.ActiveGroupViewModelTests
{
    internal class EnumerationStageViewModelTestContext
    {
        public static EnumerationStageViewModel CreateEnumerationStageViewModel(
            IInterviewViewModelFactory interviewViewModelFactory = null,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository = null,
            IStatefulInterviewRepository interviewRepository = null,
            ISubstitutionService substitutionService = null,
            ILiteEventRegistry eventRegistry = null,
            IMvxMessenger messenger = null,
            IUserInterfaceStateService userInterfaceStateService = null)
        {
            return new EnumerationStageViewModel(
                interviewViewModelFactory ?? Mock.Of<IInterviewViewModelFactory>(),
                questionnaireRepository ?? Mock.Of<IPlainKeyValueStorage<QuestionnaireModel>>(),
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                substitutionService ?? Mock.Of<ISubstitutionService>(),
                eventRegistry ?? Mock.Of<ILiteEventRegistry>(),
                messenger ?? Mock.Of<IMvxMessenger>(),
                userInterfaceStateService ?? Mock.Of<IUserInterfaceStateService>());
        }

        public static QuestionnaireReferenceModel CreateQuestionnaireReferenceModel(string id, Type modelType)
        {
            return new QuestionnaireReferenceModel { Id = Guid.Parse(id), ModelType = modelType };
        }
    }
}