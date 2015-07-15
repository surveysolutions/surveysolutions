using System;

using Cirrious.MvvmCross.Plugins.Messenger;

using Moq;

using WB.Core.BoundedContexts.Tester.Implementation.Entities;
using WB.Core.BoundedContexts.Tester.Repositories;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveySolutions.Services;

namespace WB.Tests.Unit.BoundedContexts.Tester.ViewModels.ActiveGroupViewModelTests
{
    public class ActiveGroupViewModelTestContext
    {
        public static ActiveGroupViewModel CreateActiveGroupViewModel(
            IInterviewViewModelFactory interviewViewModelFactory = null,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository = null,
            IStatefulInterviewRepository interviewRepository = null,
            ISubstitutionService substitutionService = null,
            ILiteEventRegistry eventRegistry = null,
            IMvxMessenger messenger = null)
        {
            return new ActiveGroupViewModel(
                interviewViewModelFactory ?? Mock.Of<IInterviewViewModelFactory>(),
                questionnaireRepository ?? Mock.Of<IPlainKeyValueStorage<QuestionnaireModel>>(),
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                substitutionService ?? Mock.Of<ISubstitutionService>(),
                eventRegistry ?? Mock.Of<ILiteEventRegistry>(),
                messenger ?? Mock.Of<IMvxMessenger>());
        }

        public static QuestionnaireReferenceModel CreateQuestionnaireReferenceModel(string id, Type modelType)
        {
            return new QuestionnaireReferenceModel { Id = Guid.Parse(id), ModelType = modelType };
        }
    }
}