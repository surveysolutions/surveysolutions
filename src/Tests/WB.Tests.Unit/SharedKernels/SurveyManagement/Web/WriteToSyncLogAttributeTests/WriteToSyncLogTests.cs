using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.Infrastructure.PlainStorage;
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

            SetupContext(mock.Object);

            WriteToSyncLogAttribute syncLogAttribute = Create(SynchronizationLogType.GetTranslations);
            HttpActionExecutedContext actionContext = CreateActionContext();

            // Act
            await syncLogAttribute.OnActionExecutedAsync(actionContext, new CancellationToken());

            // Assert
            mock.Verify(
               x => x.Store(Moq.It.IsAny<SynchronizationLogItem>(), Moq.It.IsAny<Guid>()), Times.Once);
        }
    }
}
