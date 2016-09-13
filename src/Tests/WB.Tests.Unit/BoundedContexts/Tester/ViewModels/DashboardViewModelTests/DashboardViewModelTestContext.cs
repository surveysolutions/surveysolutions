using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MvvmCross.Test.Core;

using Moq;
using NSubstitute;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.BoundedContexts.Tester.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Tests.Unit.BoundedContexts.Tester.ViewModels.DashboardViewModelTests
{
    internal class DashboardViewModelTestContext : MvxIoCSupportingTest
    {
        public DashboardViewModelTestContext()
        {
            base.Setup();
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
            IAsyncRunner asyncRunner = null,
            IPlainStorage<TranslationInstance> translationsStorage = null)
        {
            var userIdentity = Mock.Of<IUserIdentity>(_ => _.Name == userName && _.UserId == userId);
            mockOfPrincipal.Setup(x => x.CurrentUserIdentity).Returns(userIdentity);

            var localDashboardLastUpdateStorageMock = new Mock<IPlainStorage<DashboardLastUpdate>>();
            localDashboardLastUpdateStorageMock
                .Setup(x => x.Where(Moq.It.IsAny<Expression<Func<DashboardLastUpdate, bool>>>()))
                .Returns(
                    new List<DashboardLastUpdate>
                    {
                        new DashboardLastUpdate { Id = userName, LastUpdateDate = DateTime.Now }
                    }.ToReadOnlyCollection());

            return new DashboardViewModel(principal: mockOfPrincipal.Object,
                designerApiService: designerApiService,
                commandService: commandService,
                questionnaireImportService: questionnaireImportService,
                viewModelNavigationService: viewModelNavigationService,
                friendlyErrorMessageService: friendlyErrorMessageService,
                userInteractionService: userInteractionService,
                questionnaireListStorage: questionnaireListStorage,
                dashboardLastUpdateStorage: dashboardLastUpdateStorage ?? localDashboardLastUpdateStorageMock.Object,
                logger: logger ?? Mock.Of<ILogger>(),
                attachmentContentStorage: attachmentContentStorage ?? Mock.Of<IAttachmentContentStorage>(),
                asyncRunner: asyncRunner ?? Create.Service.AsyncRunner(),
                translationsStorage: translationsStorage ?? Mock.Of<IPlainStorage<TranslationInstance>>());
        }

        protected static readonly Guid userId = Guid.Parse("ffffffffffffffffffffffffffffffff");
        protected static readonly string userName = "Vasya";
        protected static readonly Mock<IPrincipal> mockOfPrincipal = new Mock<IPrincipal>();
    }
}