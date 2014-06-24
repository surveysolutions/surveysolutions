using System;
using System.Linq;
using System.ServiceModel;
using System.Web;
using System.Web.Http;
using Main.Core.Documents;
using Main.Core.Utility;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernel.Utils.Compression;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.Template;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Supervisor.DesignerPublicService;

namespace WB.UI.Supervisor.Controllers
{
    [Authorize(Roles = "Headquarter")]
    [Obsolete("Remove when HQ app will be separate")]
    public class DesignerQuestionnairesApiController : BaseApiController
    {
        internal DesignerPublicService.IPublicService DesignerService
        {
            get { return getDesignerService(this.GlobalInfo); }
            set { SetDesignerService(this.GlobalInfo, value); }
        }

        private readonly IStringCompressor zipUtils;
        private readonly ISupportedVersionProvider supportedVersionProvider;
        private readonly Func<IGlobalInfoProvider, DesignerPublicService.IPublicService> getDesignerService;

        public DesignerQuestionnairesApiController(
            ISupportedVersionProvider supportedVersionProvider,
            ICommandService commandService, IGlobalInfoProvider globalInfo, IStringCompressor zipUtils, ILogger logger)
            : this(supportedVersionProvider, commandService, globalInfo, zipUtils, logger, GetDesignerService) { }

        internal DesignerQuestionnairesApiController(
            ISupportedVersionProvider supportedVersionProvider,
            ICommandService commandService, IGlobalInfoProvider globalInfo, IStringCompressor zipUtils, ILogger logger,
            Func<IGlobalInfoProvider, DesignerPublicService.IPublicService> getDesignerService)
            : base(commandService, globalInfo, logger)
        {
            this.zipUtils = zipUtils;
            this.getDesignerService = getDesignerService;
            this.supportedVersionProvider = supportedVersionProvider;
        }

        private static DesignerPublicService.IPublicService GetDesignerService(IGlobalInfoProvider globalInfoProvider)
        {
            return (DesignerPublicService.IPublicService)HttpContext.Current.Session[globalInfoProvider.GetCurrentUser().Name];
        }

        private static void SetDesignerService(IGlobalInfoProvider globalInfoProvider, DesignerPublicService.IPublicService publicService)
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
                        SortOrder: StringUtil.GetOrderRequestString(data.SortOrder)));

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
            QuestionnaireDocument document = null;
            try
            {
                var supportedVerstion = supportedVersionProvider.GetSupportedQuestionnaireVersion();
                DesignerPublicService.RemoteFileInfo docSource;
                try
                {
                    docSource = this.DesignerService.DownloadQuestionnaire(new DownloadQuestionnaireRequest(request.QuestionnaireId,
                            new DesignerPublicService.QuestionnaireVersion
                            {
                                Major = supportedVerstion.Major,
                                Minor = supportedVerstion.Minor,
                                Patch = supportedVerstion.Patch
                            }));
                }
                catch (FaultException ex)
                {
                    this.Logger.Error(string.Format("Designer: error when importing template #{0}", request.QuestionnaireId), ex);
                    return new QuestionnaireVerificationResponse(true)
                    {
                        ImportError = ex.Reason.ToString()
                    };
                }

                document = this.zipUtils.Decompress<QuestionnaireDocument>(docSource.FileByteStream);

                this.CommandService.Execute(new ImportFromDesigner(this.GlobalInfo.GetCurrentUser().Id, document));

                return new QuestionnaireVerificationResponse(true);
            }
            catch (Exception ex)
            {
                var domainEx = ex.GetSelfOrInnerAs<QuestionnaireException>();
                if (domainEx == null)
                {
                    this.Logger.Error(
                        string.Format("Designer: error when importing template #{0}", request.QuestionnaireId), ex);
                    throw;
                }

                var response = new QuestionnaireVerificationResponse(true, document.Title);
                var verificationException = domainEx as QuestionnaireVerificationException;
                if (verificationException != null)
                {
                    response.SetErrorsForQuestionnaire(verificationException.Errors, document);
                }
                return response;
            }
        }

        public class ImportQuestionnaireRequest
        {
            public Guid QuestionnaireId { get; set; }
        }
    }
}