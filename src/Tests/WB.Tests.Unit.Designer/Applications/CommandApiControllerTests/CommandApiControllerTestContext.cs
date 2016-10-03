using System.Net.Http;
using System.Web.Http;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.UI.Designer.Api;
using WB.UI.Designer.Code;
using WB.UI.Shared.Web.CommandDeserialization;

namespace WB.Tests.Unit.Designer.Applications.CommandApiControllerTests
{
    internal class CommandApiControllerTestContext
    {
        public static CommandController CreateCommandController(
            ICommandService commandService = null, 
            ICommandDeserializer commandDeserializer = null, 
            ILogger logger = null, 
            ICommandInflater commandInflater = null, 
            HttpRequestMessage httpRequestMessage = null,
            HttpConfiguration httpConfiguration = null,
            IAttachmentService attachmentService = null,
            ITranslationsService translationsService = null)
        {
            var controller = new CommandController(
                commandService ?? Mock.Of<ICommandService>(),
                commandDeserializer ?? Mock.Of<ICommandDeserializer>(),
                logger ?? Mock.Of<ILogger>(),
                commandInflater ?? Mock.Of<ICommandInflater>(),
                Mock.Of<ILookupTableService>(),
                attachmentService ?? Mock.Of<IAttachmentService>(),
                translationsService ?? Mock.Of<ITranslationsService>());

            controller.Request = httpRequestMessage ?? new HttpRequestMessage(HttpMethod.Post, "https://localhost");
            controller.Request.SetConfiguration(httpConfiguration ??  new HttpConfiguration());
            
            return controller;
        }
    }
}
