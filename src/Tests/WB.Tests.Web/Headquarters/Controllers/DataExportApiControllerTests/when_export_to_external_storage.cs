using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.UI.Headquarters.Controllers.Api;

namespace WB.Tests.Web.Headquarters.Controllers.DataExportApiControllerTests
{
    internal class when_export_to_external_storage : DataExportApiControllerTestsContext
    {
        [NUnit.Framework.Test]
        public async Task when_state_was_issued_for_another_user_should_return_bad_request()
        {
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var protectionProvider = new EphemeralDataProtectionProvider();
            var issueStateController = CreateController(Guid.NewGuid(), memoryCache, protectionProvider);
            var consumeStateController = CreateController(Guid.NewGuid(), memoryCache, protectionProvider);

            var createStateResponse = issueStateController.CreateExternalStorageState(new DataExportApiController.ExternalStorageStateModel
            {
                Type = ExternalStorageType.OneDrive,
            }).Result as OkObjectResult;

            var protectedState = createStateResponse?.Value as string;

            var result = await consumeStateController.ExportToExternalStorage(new DataExportApiController.ExportToExternalStorageModel
            {
                Code = "code",
                State = protectedState
            });

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [NUnit.Framework.Test]
        public async Task when_state_is_reused_should_return_bad_request()
        {
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var protectionProvider = new EphemeralDataProtectionProvider();
            var questionnaireBrowseViewFactory = CreateQuestionnaireBrowseViewFactoryReturningNull();
            var controller = CreateController(Guid.NewGuid(), memoryCache, protectionProvider, questionnaireBrowseViewFactory);

            var createStateResponse = controller.CreateExternalStorageState(new DataExportApiController.ExternalStorageStateModel
            {
                Type = ExternalStorageType.OneDrive,
            }).Result as OkObjectResult;

            var protectedState = createStateResponse?.Value as string;

            var firstCallResult = await controller.ExportToExternalStorage(new DataExportApiController.ExportToExternalStorageModel
            {
                Code = "code",
                State = protectedState
            });
            firstCallResult.Should().BeOfType<NotFoundObjectResult>();

            var secondCallResult = await controller.ExportToExternalStorage(new DataExportApiController.ExportToExternalStorageModel
            {
                Code = "code",
                State = protectedState
            });
            secondCallResult.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}
