using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Domain;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.UI.Headquarters.Code;

namespace WB.Tests.Web.Headquarters.Filters
{
    [TestFixture]
    public class WriteToSyncLogFailureTests
    {
        private static readonly Guid UserId = Guid.NewGuid();

        private static ActionExecutingContext Context(IServiceProvider services)
        {
            var http = new DefaultHttpContext
            {
                RequestServices = services,
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, UserId.ToString()),
                    new Claim(ClaimTypes.Name, "interviewer")
                }, "test"))
            };
            return new ActionExecutingContext(new ActionContext(http, new RouteData(), new ActionDescriptor()),
                new List<IFilterMetadata>(), new Dictionary<string, object> { ["id"] = "device-123" }, new object());
        }

        [TestCase(SynchronizationLogType.CanSynchronize)]
        [TestCase(SynchronizationLogType.LinkToDevice)]
        [TestCase(SynchronizationLogType.GetAssignment)]
        public async Task should_log_failure_independently_without_resolving_request_database_services(SynchronizationLogType type)
        {
            SynchronizationLogItem saved = null;
            var storage = new Mock<IPlainStorageAccessor<SynchronizationLogItem>>();
            storage.Setup(x => x.Store(It.IsAny<SynchronizationLogItem>(), It.IsAny<object>()))
                .Callback<SynchronizationLogItem, object>((item, _) => saved = item);
            var executor = new Mock<IInScopeExecutor<IPlainStorageAccessor<SynchronizationLogItem>>>();
            executor.Setup(x => x.Execute(It.IsAny<Action<IPlainStorageAccessor<SynchronizationLogItem>>>(), null))
                .Callback<Action<IPlainStorageAccessor<SynchronizationLogItem>>, string>((action, _) => action(storage.Object));
            // No request storage or enrichment services are registered: accessing them would fail.
            using var services = new ServiceCollection().AddSingleton(executor.Object).BuildServiceProvider();
            var context = Context(services);
            var failure = new InvalidOperationException("Action failed");
            var executed = new ActionExecutedContext(context, context.Filters, context.Controller) { Exception = failure };

            await new WriteToSyncLogAttribute(type).OnActionExecutionAsync(context, () => Task.FromResult(executed));

            Assert.That(saved, Is.Not.Null);
            Assert.That(saved.InterviewerId, Is.EqualTo(UserId));
            Assert.That(saved.InterviewerName, Is.EqualTo("interviewer"));
            Assert.That(saved.Type, Is.EqualTo(type));
            Assert.That(saved.ActionExceptionType, Is.EqualTo(nameof(InvalidOperationException)));
            Assert.That(saved.ActionExceptionMessage, Is.EqualTo(failure.Message));
            Assert.That(saved.LogDate.Kind, Is.EqualTo(DateTimeKind.Utc));
            Assert.That(saved.DeviceId, Is.EqualTo(type == SynchronizationLogType.GetAssignment ? null : "device-123"));
            Assert.That(executed.Exception, Is.SameAs(failure));
            Assert.That(executed.ExceptionHandled, Is.False);
            executor.Verify(x => x.Execute(It.IsAny<Action<IPlainStorageAccessor<SynchronizationLogItem>>>(), null), Times.Once);
        }

        [Test]
        public void should_log_and_rethrow_when_action_delegate_throws()
        {
            var executor = new Mock<IInScopeExecutor<IPlainStorageAccessor<SynchronizationLogItem>>>();
            using var services = new ServiceCollection().AddSingleton(executor.Object).BuildServiceProvider();
            var context = Context(services);
            var failure = new InvalidOperationException("Action failed");

            var thrown = Assert.ThrowsAsync<InvalidOperationException>(() =>
                new WriteToSyncLogAttribute(SynchronizationLogType.LinkToDevice)
                    .OnActionExecutionAsync(context, () => throw failure));

            Assert.That(thrown, Is.SameAs(failure));
            executor.Verify(x => x.Execute(It.IsAny<Action<IPlainStorageAccessor<SynchronizationLogItem>>>(), null), Times.Once);
        }

        [Test]
        public async Task should_preserve_original_exception_when_logging_fails(
            [Values("resolve", "store", "commit", "diagnostic")] string failureStage,
            [Values(false, true)] bool delegateThrows)
        {
            var loggingFailure = new InvalidOperationException("Logging failed");
            var storage = new Mock<IPlainStorageAccessor<SynchronizationLogItem>>();
            if (failureStage == "store")
                storage.Setup(x => x.Store(It.IsAny<SynchronizationLogItem>(), It.IsAny<object>())).Throws(loggingFailure);
            var executor = new Mock<IInScopeExecutor<IPlainStorageAccessor<SynchronizationLogItem>>>();
            executor.Setup(x => x.Execute(It.IsAny<Action<IPlainStorageAccessor<SynchronizationLogItem>>>(), null))
                .Callback<Action<IPlainStorageAccessor<SynchronizationLogItem>>, string>((action, _) =>
                {
                    action(storage.Object);
                    throw loggingFailure; // Models a failure during independent scope commit/disposal.
                });
            var logger = new Mock<ILogger>();
            if (failureStage == "diagnostic")
                logger.Setup(x => x.Error(It.IsAny<string>(), It.IsAny<Exception>())).Throws(new Exception("Logger failed"));
            var provider = new Mock<ILoggerProvider>();
            provider.Setup(x => x.GetFor<WriteToSyncLogAttribute>()).Returns(logger.Object);
            using var services = new ServiceCollection()
                .AddSingleton(provider.Object)
                .AddSingleton<IInScopeExecutor<IPlainStorageAccessor<SynchronizationLogItem>>>(_ =>
                    failureStage == "resolve" ? throw loggingFailure : executor.Object)
                .BuildServiceProvider();
            var context = Context(services);
            var failure = new ApplicationException("Original action failure");
            var executed = new ActionExecutedContext(context, context.Filters, context.Controller) { Exception = failure };
            var filter = new WriteToSyncLogAttribute(SynchronizationLogType.LinkToDevice);

            if (delegateThrows)
            {
                var thrown = Assert.ThrowsAsync<ApplicationException>(() => filter.OnActionExecutionAsync(context, () => throw failure));
                Assert.That(thrown, Is.SameAs(failure));
            }
            else
            {
                await filter.OnActionExecutionAsync(context, () => Task.FromResult(executed));
                Assert.That(executed.Exception, Is.SameAs(failure));
                Assert.That(executed.ExceptionHandled, Is.False);
            }

            logger.Verify(x => x.Error(It.IsAny<string>(), loggingFailure), Times.Once);
        }

        [Test]
        public async Task should_keep_success_log_in_request_transaction()
        {
            var storage = new Mock<IPlainStorageAccessor<SynchronizationLogItem>>();
            var executor = new Mock<IInScopeExecutor<IPlainStorageAccessor<SynchronizationLogItem>>>(MockBehavior.Strict);
            using var services = new ServiceCollection()
                .AddSingleton(storage.Object).AddSingleton(executor.Object)
                .AddSingleton(Mock.Of<IQuestionnaireBrowseViewFactory>())
                .AddSingleton(Mock.Of<IQuestionnaireStorage>())
                .AddSingleton(Mock.Of<IInterviewAnswerSerializer>())
                .AddSingleton(Mock.Of<IUserToDeviceService>())
                .BuildServiceProvider();
            var context = Context(services);
            var executed = new ActionExecutedContext(context, context.Filters, context.Controller);

            await new WriteToSyncLogAttribute(SynchronizationLogType.LinkToDevice)
                .OnActionExecutionAsync(context, () => Task.FromResult(executed));

            storage.Verify(x => x.Store(It.Is<SynchronizationLogItem>(item =>
                item.ActionExceptionType == null && item.DeviceId == "device-123"), It.IsAny<object>()), Times.Once);
            executor.VerifyNoOtherCalls();
        }
    }
}
