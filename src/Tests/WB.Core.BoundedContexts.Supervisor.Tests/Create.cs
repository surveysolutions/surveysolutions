using System;
using System.Net.Http;
using Ncqrs.Commanding.ServiceModel;
using NSubstitute;
using WB.Core.BoundedContexts.Supervisor.Users.Implementation;
using WB.Core.GenericSubdomains.Logging;

namespace WB.Core.BoundedContexts.Supervisor.Tests
{
    internal static class Create
    {
        internal static HeadquartersLoginService HeadquartersLoginService(HttpMessageHandler messageHandler = null, 
            ILogger logger = null, 
            ICommandService commandService = null, 
            HeadquartersSettings headquartersSettings = null)
        {
            return new HeadquartersLoginService(logger ?? Substitute.For<ILogger>(), 
                commandService ?? Substitute.For<ICommandService>(), 
                messageHandler ?? Substitute.For<HttpMessageHandler>(),
                headquartersSettings ?? new HeadquartersSettings(new Uri("http://localhost/")));
        }
    }
}