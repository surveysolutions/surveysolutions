using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.UI.Headquarters.Filters;
using WB.UI.Shared.Web.Attributes;

namespace WB.Tests.Web.Headquarters.Filters
{
    [TestFixture]
    public class UnitOfWorkActionFilterTests
    {
        private static ActionExecutingContext Context(IServiceProvider services, params IFilterMetadata[] filters)
        {
            var httpContext = new DefaultHttpContext { RequestServices = services };
            httpContext.Response.Body = new MemoryStream();
            return new ActionExecutingContext(
                new ActionContext(httpContext, new RouteData(), new ActionDescriptor()),
                filters, new Dictionary<string, object>(), new object());
        }

        [Test]
        public async Task should_complete_after_action_and_before_first_response_write()
        {
            var calls = new List<string>();
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.Setup(x => x.AcceptChanges()).Callback(() => calls.Add("accept"));
            unitOfWork.Setup(x => x.Complete()).Callback(() => calls.Add("complete"));
            using var services = new ServiceCollection().AddSingleton(unitOfWork.Object).BuildServiceProvider();
            var context = Context(services);
            var result = new WritingResult(() => calls.Add("result"));

            await new UnitOfWorkActionFilter().OnActionExecutionAsync(context, () =>
            {
                calls.Add("action");
                return Task.FromResult(new ActionExecutedContext(context, context.Filters, context.Controller)
                {
                    Result = result
                });
            });
            await result.ExecuteResultAsync(context);

            Assert.That(calls, Is.EqualTo(new[] { "action", "accept", "complete", "result" }));
            Assert.That(context.HttpContext.Response.Body.Length, Is.GreaterThan(0));
            unitOfWork.Verify(x => x.Dispose(), Times.Never);
        }

        [Test]
        public void should_propagate_commit_failure_without_executing_success_result()
        {
            var failure = new InvalidOperationException("Commit failed");
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.Setup(x => x.Complete()).Throws(failure);
            using var services = new ServiceCollection().AddSingleton(unitOfWork.Object).BuildServiceProvider();
            var context = Context(services);
            var resultExecuted = false;
            var result = new WritingResult(() => resultExecuted = true);

            var thrown = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await new UnitOfWorkActionFilter().OnActionExecutionAsync(context, () =>
                    Task.FromResult(new ActionExecutedContext(context, context.Filters, context.Controller)
                    {
                        Result = result
                    }));
                await result.ExecuteResultAsync(context);
            });

            Assert.That(thrown, Is.SameAs(failure));
            Assert.That(resultExecuted, Is.False);
            Assert.That(context.HttpContext.Response.Body.Length, Is.Zero);
        }

        [Test]
        public async Task should_discard_and_dispose_on_action_exception()
        {
            var unitOfWork = new Mock<IUnitOfWork>();
            using var services = new ServiceCollection().AddSingleton(unitOfWork.Object).BuildServiceProvider();
            var context = Context(services);

            await new UnitOfWorkActionFilter().OnActionExecutionAsync(context, () =>
                Task.FromResult(new ActionExecutedContext(context, context.Filters, context.Controller)
                {
                    Exception = new InvalidOperationException("Action failed")
                }));

            unitOfWork.Verify(x => x.DiscardChanges(), Times.Once);
            unitOfWork.Verify(x => x.Dispose(), Times.Once);
            unitOfWork.Verify(x => x.AcceptChanges(), Times.Never);
            unitOfWork.Verify(x => x.Complete(), Times.Never);
        }

        [Test]
        public void should_discard_when_action_delegate_throws()
        {
            var unitOfWork = new Mock<IUnitOfWork>();
            using var services = new ServiceCollection().AddSingleton(unitOfWork.Object).BuildServiceProvider();
            var context = Context(services);
            var failure = new InvalidOperationException("Action failed");

            var thrown = Assert.ThrowsAsync<InvalidOperationException>(() =>
                new UnitOfWorkActionFilter().OnActionExecutionAsync(context, () => throw failure));

            Assert.That(thrown, Is.SameAs(failure));
            unitOfWork.Verify(x => x.DiscardChanges(), Times.Once);
            unitOfWork.Verify(x => x.Complete(), Times.Never);
        }

        [Test]
        public async Task should_not_resolve_unit_of_work_for_no_transaction_actions()
        {
            using var services = new ServiceCollection().BuildServiceProvider();
            var context = Context(services, new NoTransactionAttribute());
            var invoked = false;

            await new UnitOfWorkActionFilter().OnActionExecutionAsync(context, () =>
            {
                invoked = true;
                return Task.FromResult(new ActionExecutedContext(context, context.Filters, context.Controller));
            });

            Assert.That(invoked, Is.True);
        }

        private sealed class WritingResult : IActionResult
        {
            private readonly Action onExecute;

            public WritingResult(Action onExecute) => this.onExecute = onExecute;

            public async Task ExecuteResultAsync(ActionContext context)
            {
                onExecute();
                await context.HttpContext.Response.WriteAsync("Success");
            }
        }
    }
}
