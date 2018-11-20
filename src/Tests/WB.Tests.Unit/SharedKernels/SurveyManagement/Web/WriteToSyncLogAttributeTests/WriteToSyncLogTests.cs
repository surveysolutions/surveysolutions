using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Dependencies;
using System.Web.Http.Filters;
using System.Web.Http.Hosting;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.UI.Headquarters.Code;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.WriteToSyncLogAttributeTests
{
    [TestFixture]
    public class WriteToSyncLogTests : WriteToSyncLogAttributeTestsContext
    {
        [Test]
        public async Task when_getting_translations_Should_log_action()
        {
            var mock = new Mock<IPlainStorageAccessor<SynchronizationLogItem>>();
            
            WriteToSyncLogAttribute syncLogAttribute = Create(SynchronizationLogType.GetTranslations);

            var userMock = new Mock<IAuthorizedUser>();
            userMock.Setup(s => s.IsAuthenticated).Returns(true);

            var logger = new Mock<ILoggerProvider>();
            logger.Setup(x => x.GetFor<WriteToSyncLogAttribute>()).Returns(new Mock<ILogger>().Object);

            var questionnaireBrowseViewFactory = new Mock<IQuestionnaireBrowseViewFactory>();
            questionnaireBrowseViewFactory.Setup(x => x.GetById(It.IsAny<QuestionnaireIdentity>())).Returns(new QuestionnaireBrowseItem());

            var scopeMock = new Mock<IDependencyScope>();
            scopeMock.Setup(x => x.GetService(typeof(IAuthorizedUser))).Returns(userMock.Object);
            scopeMock.Setup(x => x.GetService(typeof(IPlainStorageAccessor<SynchronizationLogItem>))).Returns(mock.Object);
            scopeMock.Setup(x => x.GetService(typeof(ILoggerProvider))).Returns(logger.Object);

            scopeMock.Setup(x => x.GetService(typeof(IQuestionnaireBrowseViewFactory))).Returns(questionnaireBrowseViewFactory.Object);

            HttpActionExecutedContext actionContext = CreateActionExecutedContext();
            actionContext.Request.Properties.Add(HttpPropertyKeys.DependencyScope, scopeMock.Object);

            // Act
            await syncLogAttribute.OnActionExecutedAsync(actionContext, new CancellationToken());

            // Assert
            mock.Verify(
               x => x.Store(Moq.It.IsAny<SynchronizationLogItem>(), Moq.It.IsAny<Guid>()), Times.Once);
        }
    }
}
