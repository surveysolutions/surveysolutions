using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using WB.Core.Infrastructure.HttpServices.Services;
using WB.UI.Shared.Web.Integrity;

namespace WB.Tests.Web.Headquarters.Integrity
{
    [TestOf(typeof(IntegrityHeaderMiddleware))]
    public class IntegrityHeaderMiddlewareTests
    {
        [Test]
        public async Task when_request_completes_successfully_should_add_integrity_header()
        {
            var context = CreateHttpContext("/api/ping");
            var middleware = CreateMiddleware(async httpContext =>
            {
                httpContext.Response.StatusCode = StatusCodes.Status200OK;
                await httpContext.Response.CompleteAsync();
            });

            await middleware.InvokeAsync(context);

            context.Response.Headers.Should().ContainKey(IntegrityService.IntegrityHeaderName);
            context.Response.Headers[IntegrityService.IntegrityHeaderName].ToString().Should().Be("test-integrity");
            context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

        [Test]
        public async Task when_api_request_throws_and_response_not_started_should_return_500_with_integrity_header()
        {
            var context = CreateHttpContext("/api/fail");
            var middleware = CreateMiddleware(_ => throw new InvalidOperationException("boom"));

            await middleware.InvokeAsync(context);

            context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            context.Response.Headers.Should().ContainKey(IntegrityService.IntegrityHeaderName);
            context.Response.Headers[IntegrityService.IntegrityHeaderName].ToString().Should().Be("test-integrity");
        }

        [Test]
        public async Task when_graphql_request_throws_should_return_500_with_integrity_header()
        {
            var context = CreateHttpContext("/graphql");
            var middleware = CreateMiddleware(_ => throw new InvalidOperationException("boom"));

            await middleware.InvokeAsync(context);

            context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            context.Response.Headers.Should().ContainKey(IntegrityService.IntegrityHeaderName);
            context.Response.Headers[IntegrityService.IntegrityHeaderName].ToString().Should().Be("test-integrity");
        }

        [Test]
        public async Task when_xhr_request_throws_should_return_500_with_integrity_header()
        {
            var context = CreateHttpContext("/dashboard");
            context.Request.Headers["X-Requested-With"] = "XMLHttpRequest";
            var middleware = CreateMiddleware(_ => throw new InvalidOperationException("boom"));

            await middleware.InvokeAsync(context);

            context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            context.Response.Headers.Should().ContainKey(IntegrityService.IntegrityHeaderName);
            context.Response.Headers[IntegrityService.IntegrityHeaderName].ToString().Should().Be("test-integrity");
        }

        [Test]
        public void when_non_api_request_throws_should_rethrow_original_exception()
        {
            var context = CreateHttpContext("/Reports/Index");
            var expected = new InvalidOperationException("boom");
            var middleware = CreateMiddleware(_ => throw expected);

            var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await middleware.InvokeAsync(context));

            exception.Should().BeSameAs(expected);
        }

        [Test]
        public void when_response_has_started_and_request_throws_should_rethrow_original_exception()
        {
            var context = CreateHttpContext("/api/fail-after-start");
            var expected = new InvalidOperationException("boom");
            var middleware = CreateMiddleware(async httpContext =>
            {
                await httpContext.Response.StartAsync();
                throw expected;
            });

            var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await middleware.InvokeAsync(context));

            exception.Should().BeSameAs(expected);
        }

        private static IntegrityHeaderMiddleware CreateMiddleware(RequestDelegate next)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["Integrity:HeaderValue"] = "test-integrity"
                })
                .Build();

            return new IntegrityHeaderMiddleware(next, configuration, Mock.Of<ILogger<IntegrityHeaderMiddleware>>());
        }

        private static HttpContext CreateHttpContext(string path)
        {
            var context = new DefaultHttpContext();
            context.Request.Path = path;

            // Wire up proper response lifecycle so OnStarting callbacks fire and
            // HasStarted is set correctly — DefaultHttpContext alone does not do this.
            var responseFeature = new TestHttpResponseFeature();
            context.Features.Set<IHttpResponseFeature>(responseFeature);
            context.Features.Set<IHttpResponseBodyFeature>(new TestHttpResponseBodyFeature(responseFeature));

            return context;
        }

        /// <summary>
        /// Replacement for the default <see cref="HttpResponseFeature"/> that tracks
        /// whether the response has started and fires <c>OnStarting</c> callbacks when
        /// <see cref="TriggerOnStartingAsync"/> is called (normally done by the server).
        /// </summary>
        private class TestHttpResponseFeature : HttpResponseFeature
        {
            private bool hasStarted;
            private readonly List<(Func<object, Task> Callback, object State)> callbacks = new();

            public override bool HasStarted => hasStarted;

            public override void OnStarting(Func<object, Task> callback, object state)
            {
                if (hasStarted)
                    throw new InvalidOperationException("Response has already started.");
                callbacks.Add((callback, state));
            }

            public async Task TriggerOnStartingAsync()
            {
                if (hasStarted) return;
                // Execute OnStarting callbacks in LIFO order and allow callbacks to register additional callbacks.
                while (callbacks.Count > 0)
                {
                    var (cb, state) = callbacks[^1];
                    callbacks.RemoveAt(callbacks.Count - 1);
                    await cb(state);
                }
                hasStarted = true;
            }
        }

        /// <summary>
        /// Replacement body feature that calls <see cref="TestHttpResponseFeature.TriggerOnStartingAsync"/>
        /// when <see cref="StartAsync"/> or <see cref="CompleteAsync"/> is called, matching real-server behaviour.
        /// </summary>
        private class TestHttpResponseBodyFeature : IHttpResponseBodyFeature
        {
            private readonly TestHttpResponseFeature responseFeature;

            public TestHttpResponseBodyFeature(TestHttpResponseFeature responseFeature)
                => this.responseFeature = responseFeature;

            public Stream Stream => Stream.Null;
            public System.IO.Pipelines.PipeWriter Writer
                => System.IO.Pipelines.PipeWriter.Create(Stream.Null);

            public async Task StartAsync(CancellationToken cancellationToken = default)
                => await responseFeature.TriggerOnStartingAsync();

            public async Task CompleteAsync()
                => await responseFeature.TriggerOnStartingAsync();

            public Task SendFileAsync(string path, long offset, long? count, CancellationToken cancellationToken = default)
                => Task.CompletedTask;

            public void DisableBuffering() { }
        }
    }
}

