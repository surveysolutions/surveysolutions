using Cirrious.MvvmCross.Test.Core;

using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.BoundedContexts.Tester.Views;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Tests.Unit.BoundedContexts.Tester.ViewModels.DashboardViewModelTests
{
    public class DashboardViewModelTestContext : MvxIoCSupportingTest
    {
        public DashboardViewModelTestContext()
        {
            base.Setup();
        }

        public static DashboardViewModel CreateDashboardViewModel(IPrincipal principal = null,
            ILogger logger = null,
            IDesignerApiService designerApiService = null,
            ICommandService commandService = null,
            IQuestionnaireImportService questionnaireImportService = null,
            IViewModelNavigationService viewModelNavigationService = null,
            IFriendlyErrorMessageService friendlyErrorMessageService = null,
            IUserInteractionService userInteractionService = null,
            IPlainStorageAccessor<QuestionnaireListItem> questionnaireListStorageAccessor = null)
        {
            return new DashboardViewModel(principal: principal,
                designerApiService: designerApiService,
                commandService: commandService,
                questionnaireImportService: questionnaireImportService,
                viewModelNavigationService: viewModelNavigationService,
                friendlyErrorMessageService: friendlyErrorMessageService,
                userInteractionService: userInteractionService,
                questionnaireListStorageAccessor: questionnaireListStorageAccessor);
        }
    }
}