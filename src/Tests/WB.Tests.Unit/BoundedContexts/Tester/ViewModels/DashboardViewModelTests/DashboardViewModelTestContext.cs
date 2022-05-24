using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Main.Core.Documents;
using Moq;
using MvvmCross.Tests;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.BoundedContexts.Tester.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Tester.ViewModels.DashboardViewModelTests
{
    internal class DashboardViewModelTestContext : MvxIoCSupportingTest
    {
        public DashboardViewModelTestContext()
        {
            Setup();
        }

        public static DashboardViewModel CreateDashboardViewModel(
            ILogger logger = null,
            IDesignerApiService designerApiService = null,
            ICommandService commandService = null,
            IQuestionnaireImportService questionnaireImportService = null,
            IViewModelNavigationService viewModelNavigationService = null,
            IFriendlyErrorMessageService friendlyErrorMessageService = null,
            IUserInteractionService userInteractionService = null,
            IPlainStorage<QuestionnaireListItem> questionnaireListStorage = null,
            IPlainStorage<DashboardLastUpdate> dashboardLastUpdateStorage = null,
            IAttachmentContentStorage attachmentContentStorage = null,
            IQuestionnaireStorage questionnaireRepository = null)
        {
            var userIdentity = Mock.Of<IUserIdentity>(_ => _.Name == userName && _.UserId == userId);
            mockOfPrincipal.Setup(x => x.CurrentUserIdentity).Returns(userIdentity);
            mockOfPrincipal.Setup(x => x.IsAuthenticated).Returns(true);

            var localDashboardLastUpdateStorageMock = new Mock<IPlainStorage<DashboardLastUpdate>>();
            localDashboardLastUpdateStorageMock
                .Setup(x => x.Where(It.IsAny<Expression<Func<DashboardLastUpdate, bool>>>()))
                .Returns(
                    new List<DashboardLastUpdate>
                    {
                        new DashboardLastUpdate { Id = userName, LastUpdateDate = DateTime.Now }
                    }.ToReadOnlyCollection());

            return new DashboardViewModel(
                principal: mockOfPrincipal.Object,
                designerApiService: designerApiService,
                viewModelNavigationService: viewModelNavigationService,
                friendlyErrorMessageService: friendlyErrorMessageService,
                userInteractionService: userInteractionService,
                questionnaireListStorage: questionnaireListStorage,
                dashboardLastUpdateStorage: dashboardLastUpdateStorage ?? localDashboardLastUpdateStorageMock.Object,
                logger: logger ?? Mock.Of<ILogger>(),
                questionnaireDownloader: new QuestionnaireDownloadViewModel(
                    principal: mockOfPrincipal.Object,
                    designerApiService: designerApiService,
                    commandService: commandService,
                    questionnaireImportService: questionnaireImportService,
                    viewModelNavigationService: viewModelNavigationService,
                    friendlyErrorMessageService: friendlyErrorMessageService,
                    userInteractionService: userInteractionService,
                    logger: logger ?? Mock.Of<ILogger>(),
                    attachmentContentStorage: attachmentContentStorage ?? Mock.Of<IAttachmentContentStorage>(),
                    questionnaireRepository: questionnaireRepository ?? Create.Fake.QuestionnaireRepository(new KeyValuePair<string, QuestionnaireDocument>[]{}),
                    executedCommandsStorage: Mock.Of<IExecutedCommandsStorage>()
                ));
        }

        protected static readonly Guid userId = Id.gF;
        protected static readonly string userName = "Vasya";
        protected static readonly Mock<ITesterPrincipal> mockOfPrincipal = new Mock<ITesterPrincipal>();
    }
}
