using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using WB.Enumerator.Native.WebInterview.Pipeline;
using WB.Infrastructure.Native.Workspaces;
using WebInterviewHub = WB.Enumerator.Native.WebInterview.WebInterview;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Native.WebInterview
{
    [TestFixture]
    public class WebInterviewTests
    {
        [TestCase(true, false)]
        [TestCase(false, false)]
        [TestCase(true, true)]
        [TestCase(false, true)]
        public async Task should_disconnect_after_request_scope_is_disposed(bool hasWorkspace, bool throwOnDisconnect)
        {
            var services = new ServiceCollection();
            if (hasWorkspace)
            {
                services.AddScoped<TestWorkspaceContext>();
                services.AddScoped<IWorkspaceContextAccessor>(sp => sp.GetRequiredService<TestWorkspaceContext>());
                services.AddScoped<IWorkspaceContextSetter>(sp => sp.GetRequiredService<TestWorkspaceContext>());
            }

            var modules = new List<RecordingPipelineModule>();
            var cleanupError = throwOnDisconnect ? new InvalidOperationException("Cleanup failed") : null;
            services.AddScoped<IPipelineModule>(sp =>
            {
                var module = new RecordingPipelineModule(
                    sp.GetService<IWorkspaceContextAccessor>()?.CurrentWorkspace(), cleanupError);
                modules.Add(module);
                return module;
            });

            using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
            using var requestScope = provider.CreateScope();
            var workspace = hasWorkspace ? new WorkspaceContext("survey", "Survey") : null;
            requestScope.ServiceProvider.GetService<IWorkspaceContextSetter>()?.Set(workspace);

            var interviewId = Guid.NewGuid().ToString();
            var httpContext = new DefaultHttpContext { RequestServices = requestScope.ServiceProvider };
            httpContext.Request.QueryString = new QueryString($"?interviewId={interviewId}&mode=review");
            var features = new FeatureCollection();
            features.Set(Mock.Of<IHttpContextFeature>(x => x.HttpContext == httpContext));
            var items = new Dictionary<object, object> { ["sectionId"] = null };
            var context = Mock.Of<HubCallerContext>(x => x.Features == features
                && x.Items == items && x.ConnectionId == "connection");
            var groups = new Mock<IGroupManager>();
            groups.Setup(x => x.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            groups.Setup(x => x.RemoveFromGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // SignalR creates separate hub instances and service scopes for lifecycle callbacks.
            using (var connectedScope = provider.CreateScope())
            using (var connectedHub = new WebInterviewHub(connectedScope.ServiceProvider)
                   { Context = context, Groups = groups.Object })
            {
                await connectedHub.OnConnectedAsync();
            }

            Assert.That(modules.Count, Is.EqualTo(1));
            Assert.That(modules[0].Connected, Is.True);
            Assert.That(modules[0].Workspace, Is.SameAs(workspace));
            requestScope.Dispose();
            Assert.That(modules[0].Disposed, Is.True);
            Assert.Throws<ObjectDisposedException>(() => httpContext.RequestServices.GetServices<IPipelineModule>());

            using var disconnectedScope = provider.CreateScope();
            using var disconnectedHub = new WebInterviewHub(disconnectedScope.ServiceProvider)
                { Context = context, Groups = groups.Object };
            var disconnectError = new Exception("Connection lost");
            if (throwOnDisconnect)
            {
                var error = Assert.ThrowsAsync<InvalidOperationException>(() => disconnectedHub.OnDisconnectedAsync(disconnectError));
                Assert.That(error, Is.SameAs(cleanupError));
            }
            else
            {
                await disconnectedHub.OnDisconnectedAsync(disconnectError);
                groups.Verify(x => x.RemoveFromGroupAsync("connection", interviewId, It.IsAny<CancellationToken>()), Times.Once);
                groups.Verify(x => x.RemoveFromGroupAsync("connection",
                    WebInterviewHub.GetConnectedClientPrefilledSectionKey(Guid.Parse(interviewId)),
                    It.IsAny<CancellationToken>()), Times.Once);
            }

            Assert.That(modules.Count, Is.EqualTo(2));
            Assert.That(modules[1].Workspace, Is.SameAs(workspace));
            Assert.That(modules[1].DisconnectException, Is.SameAs(disconnectError));
            Assert.That(modules[1].Disposed, Is.True);
        }

        private class TestWorkspaceContext : IWorkspaceContextAccessor, IWorkspaceContextSetter
        {
            private WorkspaceContext workspace;
            public WorkspaceContext CurrentWorkspace() => workspace;
            public void Set(WorkspaceContext context) => workspace = context;
            public void Set(string name) => throw new NotSupportedException();
        }

        private class RecordingPipelineModule : IPipelineModule, IDisposable
        {
            private readonly Exception cleanupError;

            public RecordingPipelineModule(WorkspaceContext workspace, Exception cleanupError)
            {
                Workspace = workspace;
                this.cleanupError = cleanupError;
            }

            public WorkspaceContext Workspace { get; }
            public bool Connected { get; private set; }
            public Exception DisconnectException { get; private set; }
            public bool Disposed { get; private set; }

            public Task OnConnected(Hub hub)
            {
                Connected = true;
                return Task.CompletedTask;
            }

            public async Task OnDisconnected(Hub hub, Exception exception)
            {
                await Task.Yield();
                Assert.That(Disposed, Is.False);
                DisconnectException = exception;
                if (cleanupError != null)
                    throw cleanupError;
            }

            public void Dispose() => Disposed = true;
        }
    }
}

