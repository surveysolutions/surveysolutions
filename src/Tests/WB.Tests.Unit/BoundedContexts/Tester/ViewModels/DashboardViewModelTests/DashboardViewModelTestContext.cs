﻿using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.BoundedContexts.Tester.Infrastructure;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.BoundedContexts.Tester.Views;
using WB.Core.Infrastructure.CommandBus;

namespace WB.Tests.Unit.BoundedContexts.Tester.ViewModels.DashboardViewModelTests
{
    public class DashboardViewModelTestContext
    {
        public static DashboardViewModel CreateDashboardViewModel(IPrincipal principal = null,
            ILogger logger = null,
            IDesignerApiService designerApiService = null,
            ICommandService commandService = null,
            IQuestionnaireImportService questionnaireImportService = null,
            IViewModelNavigationService viewModelNavigationService = null,
            IFriendlyMessageService friendlyMessageService = null,
            IUserInteractionService userInteractionService = null,
            IPlainStorageAccessor<QuestionnaireListItem> questionnaireListStorageAccessor = null)
        {
            return new DashboardViewModel(principal: principal,
                logger: logger,
                designerApiService: designerApiService,
                commandService: commandService,
                questionnaireImportService: questionnaireImportService,
                viewModelNavigationService: viewModelNavigationService,
                friendlyMessageService: friendlyMessageService,
                userInteractionService: userInteractionService,
                questionnaireListStorageAccessor: questionnaireListStorageAccessor);
        }
    }
}