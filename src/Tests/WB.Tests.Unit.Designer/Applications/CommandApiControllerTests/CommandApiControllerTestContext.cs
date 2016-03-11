﻿using System.Net.Http;
using System.Web.Http;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.UI.Designer.Api;
using WB.UI.Designer.Code;
using WB.UI.Shared.Web.CommandDeserialization;

namespace WB.Tests.Unit.Applications.CommandApiControllerTests
{
    internal class CommandApiControllerTestContext
    {
        public static CommandController CreateCommandController(
            ICommandService commandService = null, 
            ICommandDeserializer commandDeserializer = null, 
            ILogger logger = null, 
            ICommandInflater commandInflater = null, 
            ICommandPostprocessor commandPostprocessor = null,
            HttpRequestMessage httpRequestMessage = null,
            HttpConfiguration httpConfiguration = null)
        {
            var controller = new CommandController(
                commandService ?? Mock.Of<ICommandService>(),
                commandDeserializer ?? Mock.Of<ICommandDeserializer>(),
                logger ?? Mock.Of<ILogger>(),
                commandInflater ?? Mock.Of<ICommandInflater>(),
                commandPostprocessor ?? Mock.Of<ICommandPostprocessor>(),
                Mock.Of<ILookupTableService>(),
                Mock.Of<IAttachmentService>());

            controller.Request = httpRequestMessage ?? new HttpRequestMessage(HttpMethod.Post, "https://localhost");
            controller.Request.SetConfiguration(httpConfiguration ??  new HttpConfiguration());
            
            return controller;
        }
    }
}
