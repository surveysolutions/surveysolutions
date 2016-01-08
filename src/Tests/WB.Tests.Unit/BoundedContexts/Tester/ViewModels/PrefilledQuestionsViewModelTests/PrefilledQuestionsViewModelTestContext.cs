using System;
using System.Collections.Generic;
using System.Linq;
using Cirrious.MvvmCross.Test.Core;
using Moq;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.BoundedContexts.Tester.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Tests.Unit.BoundedContexts.Tester.ViewModels.PrefilledQuestionsViewModelTests
{
    internal class PrefilledQuestionsViewModelTestContext : MvxIoCSupportingTest
    {
        public PrefilledQuestionsViewModelTestContext()
        {
            base.Setup();
        }

        public static PrefilledQuestionsViewModel CreatePrefilledQuestionsViewModel(
            IInterviewViewModelFactory interviewViewModelFactory = null,
            IPlainKeyValueStorage<QuestionnaireModel> plainQuestionnaireRepository = null,
            IStatefulInterviewRepository interviewRepository = null,
            IViewModelNavigationService viewModelNavigationService = null,
            ILogger logger = null)
        {
            return new PrefilledQuestionsViewModel(
                interviewViewModelFactory ?? Mock.Of<IInterviewViewModelFactory>(),
                Mock.Of<IPlainQuestionnaireRepository>(),
                plainQuestionnaireRepository ?? Mock.Of<IPlainKeyValueStorage<QuestionnaireModel>>(),
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                viewModelNavigationService ?? Mock.Of<IViewModelNavigationService>(),
                logger ?? Mock.Of<ILogger>());
        }
    }
}