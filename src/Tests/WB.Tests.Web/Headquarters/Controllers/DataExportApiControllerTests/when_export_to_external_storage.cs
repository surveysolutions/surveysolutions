using System;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
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
            var userId = Guid.NewGuid();
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var protectionProvider = new EphemeralDataProtectionProvider();
            var questionnaireBrowseViewFactory = CreateQuestionnaireBrowseViewFactoryReturningNull();
            var controller = CreateController(memoryCache, protectionProvider, questionnaireBrowseViewFactory);
            SetAuthenticatedUser(controller, userId);

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

        [NUnit.Framework.Test]
        public async Task when_state_is_redeemed_concurrently_should_only_process_once()
        {
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var protectionProvider = new EphemeralDataProtectionProvider();
            var questionnaireBrowseViewFactory = CreateQuestionnaireBrowseViewFactoryReturningNull();
            var controller = CreateController(memoryCache, protectionProvider, questionnaireBrowseViewFactory);
            SetAuthenticatedUser(controller, Guid.NewGuid());

            var createStateResponse = controller.CreateExternalStorageState(new DataExportApiController.ExternalStorageStateModel
            {
                Type = ExternalStorageType.OneDrive,
            }).Result as OkObjectResult;

            var protectedState = createStateResponse?.Value as string;
            var callbackModel = new DataExportApiController.ExportToExternalStorageModel
            {
                Code = "code",
                State = protectedState
            };

            var results = await Task.WhenAll(
                controller.ExportToExternalStorage(callbackModel),
                controller.ExportToExternalStorage(callbackModel));

            results.Count(x => x is NotFoundObjectResult).Should().Be(1);
            results.Count(x => x is BadRequestObjectResult).Should().Be(1);
        }

        [NUnit.Framework.Test]
        public async Task when_anonymous_callback_redeems_state_should_process_state()
        {
            var ownerUserId = Guid.NewGuid();
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var protectionProvider = new EphemeralDataProtectionProvider();
            var questionnaireBrowseViewFactory = CreateQuestionnaireBrowseViewFactoryReturningNull();

            var ownerController = CreateController(memoryCache, protectionProvider, questionnaireBrowseViewFactory);
            SetAuthenticatedUser(ownerController, ownerUserId);

            var createStateResponse = ownerController.CreateExternalStorageState(new DataExportApiController.ExternalStorageStateModel
            {
                Type = ExternalStorageType.OneDrive,
            }).Result as OkObjectResult;

            var protectedState = createStateResponse?.Value as string;

            var callbackController = CreateController(memoryCache, protectionProvider, questionnaireBrowseViewFactory);

            var result = await callbackController.ExportToExternalStorage(new DataExportApiController.ExportToExternalStorageModel
            {
                Code = "code",
                State = protectedState
            });
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [NUnit.Framework.Test]
        public async Task when_different_user_redeems_state_should_process_state()
        {
            var ownerUserId = Guid.NewGuid();
            var differentUserId = Guid.NewGuid();
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var protectionProvider = new EphemeralDataProtectionProvider();
            var questionnaireBrowseViewFactory = CreateQuestionnaireBrowseViewFactoryReturningNull();

            var ownerController = CreateController(memoryCache, protectionProvider, questionnaireBrowseViewFactory);
            SetAuthenticatedUser(ownerController, ownerUserId);

            var createStateResponse = ownerController.CreateExternalStorageState(new DataExportApiController.ExternalStorageStateModel
            {
                Type = ExternalStorageType.OneDrive,
            }).Result as OkObjectResult;

            var protectedState = createStateResponse?.Value as string;

            var callbackController = CreateController(memoryCache, protectionProvider, questionnaireBrowseViewFactory);
            SetAuthenticatedUser(callbackController, differentUserId);

            var result = await callbackController.ExportToExternalStorage(new DataExportApiController.ExportToExternalStorageModel
            {
                Code = "code",
                State = protectedState
            });
            result.Should().BeOfType<NotFoundObjectResult>();
        }
    }
}
