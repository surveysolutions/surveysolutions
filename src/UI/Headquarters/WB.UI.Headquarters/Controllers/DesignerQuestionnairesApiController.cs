using System;
using System.Linq;
using System.ServiceModel;
using System.Web;
using System.Web.Http;
using Main.Core.Documents;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernel.Utils.Compression;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.Template;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.UI.Headquarters.PublicService;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Headquarter")]
    [ApiValidationAntiForgeryToken]
    public class DesignerQuestionnairesApiController : BaseApiController
    {
        internal IPublicService DesignerService
        {
            get { return this.getDesignerService(this.GlobalInfo); }
            set { SetDesignerService(this.GlobalInfo, value); }
        }

        private readonly IStringCompressor zipUtils;
        private readonly ISupportedVersionProvider supportedVersionProvider;
        private readonly Func<IGlobalInfoProvider, IPublicService> getDesignerService;

        public DesignerQuestionnairesApiController(
            ISupportedVersionProvider supportedVersionProvider,
            ICommandService commandService, IGlobalInfoProvider globalInfo, IStringCompressor zipUtils, ILogger logger)
            : this(supportedVersionProvider, commandService, globalInfo, zipUtils, logger, GetDesignerService) { }

        internal DesignerQuestionnairesApiController(
            ISupportedVersionProvider supportedVersionProvider,
            ICommandService commandService, IGlobalInfoProvider globalInfo, IStringCompressor zipUtils, ILogger logger,
            Func<IGlobalInfoProvider, IPublicService> getDesignerService)
            : base(commandService, globalInfo, logger)
        {
            this.zipUtils = zipUtils;
            this.getDesignerService = getDesignerService;
            this.supportedVersionProvider = supportedVersionProvider;
        }

        private static IPublicService GetDesignerService(IGlobalInfoProvider globalInfoProvider)
        {
            return (IPublicService)HttpContext.Current.Session[globalInfoProvider.GetCurrentUser().Name];
        }

        private static void SetDesignerService(IGlobalInfoProvider globalInfoProvider, IPublicService publicService)
        {
            HttpContext.Current.Session[globalInfoProvider.GetCurrentUser().Name] = publicService;
        }

        public DesignerQuestionnairesView QuestionnairesList(DesignerQuestionnairesListViewModel data)
        {
            QuestionnaireListViewMessage list =
                this.DesignerService.GetQuestionnaireList(
                    new QuestionnaireListRequest(
                        Filter: data.Request.Filter,
                        PageIndex: data.Pager.Page,
                        PageSize: data.Pager.PageSize,
                        SortOrder: data.SortOrder.GetOrderRequestString()));

            return new DesignerQuestionnairesView()
                {
                    Items = list.Items.Select(x => new DesignerQuestionnaireListViewItem() { Id = x.Id, Title = x.Title }),
                    TotalCount = list.TotalCount,
                    ItemsSummary = null
                };
        }

        [HttpPost]
        public QuestionnaireVerificationResponse GetQuestionnaire(ImportQuestionnaireRequest request)
        {
            try
            {
                var supportedVerstion = this.supportedVersionProvider.GetSupportedQuestionnaireVersion();

                RemoteFileInfo docSource =
                    this.DesignerService.DownloadQuestionnaire(new DownloadQuestionnaireRequest(request.QuestionnaireId,
                        new QuestionnaireVersion
                        {
                            Major = supportedVerstion.SupportedQuestionnaireVersionMajor,
                            Minor = supportedVerstion.SupportedQuestionnaireVersionMinor,
                            Patch = supportedVerstion.SupportedQuestionnaireVersionPatch
                        }));

                var document = this.zipUtils.Decompress<QuestionnaireDocument>(docSource.FileByteStream);

                var supportingAssembly = docSource.SupportingAssembly;

                this.CommandService.Execute(new ImportFromDesigner(this.GlobalInfo.GetCurrentUser().Id, document,
                    request.AllowCensusMode, supportingAssembly));

                return new QuestionnaireVerificationResponse();
            }
            catch (FaultException ex)
            {
                this.Logger.Error(
                    string.Format("Designer: error when importing template #{0}", request.QuestionnaireId), ex);

                return new QuestionnaireVerificationResponse() {ImportError = ex.Reason.ToString()};
            }
            catch (Exception ex)
            {
                var domainEx = ex.GetSelfOrInnerAs<QuestionnaireException>();
                if (domainEx != null) return new QuestionnaireVerificationResponse() {ImportError = domainEx.Message};

                this.Logger.Error(
                    string.Format("Designer: error when importing template #{0}", request.QuestionnaireId), ex);
                throw;
            }
        }
    }
}