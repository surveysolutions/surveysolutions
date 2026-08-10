using System;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Headquarters.DataExport;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.UI.Headquarters.Controllers.Api;

namespace WB.Tests.Web.Headquarters.Controllers.DataExportApiControllerTests
{
    [NUnit.Framework.TestOf(typeof(DataExportApiController))]
    internal class DataExportApiControllerTestsContext
    {
        protected static DataExportApiController CreateController(
            Guid userId,
            IMemoryCache memoryCache,
            IDataProtectionProvider protectionProvider,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory = null)
        {
            var authorizedUser = Mock.Of<IAuthorizedUser>(x => x.Id == userId);

            return new DataExportApiController(
                Mock.Of<IFileSystemAccessor>(),
                Mock.Of<IDataExportStatusReader>(),
                new TestSerializer(),
                Mock.Of<IExportSettings>(),
                questionnaireBrowseViewFactory ?? Mock.Of<IQuestionnaireBrowseViewFactory>(),
                Mock.Of<IExportFileNameService>(),
                Mock.Of<IExportServiceApi>(),
                Mock.Of<ISystemLog>(),
                new ExternalStoragesSettings(),
                authorizedUser,
                protectionProvider,
                memoryCache,
                Mock.Of<ILogger<DataExportApiController>>());
        }

        protected static IQuestionnaireBrowseViewFactory CreateQuestionnaireBrowseViewFactoryReturningNull()
        {
            var questionnaireBrowseViewFactory = new Mock<IQuestionnaireBrowseViewFactory>();
            questionnaireBrowseViewFactory.Setup(x => x.GetById(It.IsAny<QuestionnaireIdentity>()))
                .Returns((QuestionnaireBrowseItem) null);
            return questionnaireBrowseViewFactory.Object;
        }

        protected class TestSerializer : ISerializer
        {
            public string Serialize(object item) => JsonConvert.SerializeObject(item);
            public string SerializeWithoutTypes(object item) => JsonConvert.SerializeObject(item);
            public T DeserializeWithoutTypes<T>(string payload) => JsonConvert.DeserializeObject<T>(payload);
            public T Deserialize<T>(string payload) => JsonConvert.DeserializeObject<T>(payload);
        }
    }
}
