using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
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
        public void should_allow_anonymous_oauth_callback()
        {
            var callback = typeof(DataExportApiController)
                .GetMethod(nameof(DataExportApiController.ExportToExternalStorage));

            callback.Should().NotBeNull();
            callback!.GetCustomAttribute<AllowAnonymousAttribute>().Should().NotBeNull();
        }

        [NUnit.Framework.Test]
        public async Task when_state_is_reused_should_return_bad_request()
        {
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var protectionProvider = new EphemeralDataProtectionProvider();
            var questionnaireBrowseViewFactory = CreateQuestionnaireBrowseViewFactoryReturningNull();
            var controller = CreateController(memoryCache, protectionProvider, questionnaireBrowseViewFactory);

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
