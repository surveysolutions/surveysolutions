using System;
using Microsoft.AspNet.SignalR;
using Moq;

namespace WB.Tests.Unit
{
    public class SignalrResolverMock : IDisposable
    {
        readonly IDependencyResolver nestedResolver;

        public Mock<IDependencyResolver> ResolverMock { get; } = new Mock<IDependencyResolver>();

        public SignalrResolverMock()
        {
            nestedResolver = GlobalHost.DependencyResolver;
            GlobalHost.DependencyResolver = ResolverMock.Object;
        }

        public void Mock<TService, TImpl>(TImpl instance)
        {
            ResolverMock.Setup(r => r.GetService(typeof(TService))).Returns(instance);
        }

        public void Dispose()
        {
            GlobalHost.DependencyResolver = nestedResolver;
        }
    }
}