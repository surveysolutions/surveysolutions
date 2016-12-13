using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Questionnaires.Translations;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.UI.Headquarters.Controllers;

namespace WB.Tests.Unit.Applications.Headquarters.DesignerQuestionnairesApiControllerTests
{
    [Subject(typeof(DesignerQuestionnairesApiController))]
    internal class DesignerQuestionnairesApiControllerTestsContext
    {
        protected static DesignerQuestionnairesApiController CreateDesignerQuestionnairesApiController(
            ICommandService commandService = null,
            IGlobalInfoProvider globalInfo = null,
            IStringCompressor zipUtils = null,
            ILogger logger = null,
            Func<IGlobalInfoProvider, RestCredentials> getDesignerUserCredentials = null,
            IRestService restService = null,
            ISupportedVersionProvider supportedVersionProvider = null,
            IAttachmentContentService attachmentContentService = null,
            IPlainStorageAccessor<TranslationInstance> translationInstances = null,
            IQuestionnaireVersionProvider questionnaireVersionProvider = null
            )
        {
            var designerUserCredentials = getDesignerUserCredentials ?? (_ => new Mock<RestCredentials> {DefaultValue = DefaultValue.Mock}.Object);

            var service = restService ?? Mock.Of<IRestService>();
            var globalInfoProvider = globalInfo ?? new Mock<IGlobalInfoProvider> {DefaultValue = DefaultValue.Mock}.Object;
            var questionnaireImportService = new QuestionnaireImportService(
                supportedVersionProvider ?? Mock.Of<ISupportedVersionProvider>(),
                service,
                globalInfoProvider,
                zipUtils ?? new Mock<IStringCompressor> { DefaultValue = DefaultValue.Mock }.Object,
                attachmentContentService ?? Mock.Of<IAttachmentContentService>(),
                questionnaireVersionProvider ?? Mock.Of<IQuestionnaireVersionProvider>(),
                Mock.Of<ITranslationManagementService>(),
                commandService ?? Mock.Of<ICommandService>(),
                Mock.Of<ILogger>()
            );
            return new DesignerQuestionnairesApiController(commandService ?? Mock.Of<ICommandService>(),
                globalInfoProvider,
                logger ?? Mock.Of<ILogger>(),
                designerUserCredentials,
                service,
                questionnaireImportService);
        }
    }
}