using System;
using System.Linq;
using System.Web;
using System.Web.Http;
using Core.Supervisor.Views.Template;
using Main.Core.Documents;
using Main.Core.Utility;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernel.Utils.Compression;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.UI.Shared.Web;
using WB.UI.Shared.Web.Extensions;
using Web.Supervisor.DesignerPublicService;
using Web.Supervisor.Models;

namespace Web.Supervisor.Controllers
{
    [Authorize(Roles = "Headquarter")]
    public class DesignerQuestionnairesApiController : BaseApiController
    {
        private IPublicService DesignerService
        {
            get { return getDesignerService(this.GlobalInfo); }
        }

        private readonly IStringCompressor zipUtils;
        private readonly Func<IGlobalInfoProvider, IPublicService> getDesignerService;

        public DesignerQuestionnairesApiController(
            ICommandService commandService, IGlobalInfoProvider globalInfo, IStringCompressor zipUtils, ILogger logger)
            : this(commandService, globalInfo, zipUtils, logger, GetDesignerService) { }

        internal DesignerQuestionnairesApiController(
            ICommandService commandService, IGlobalInfoProvider globalInfo, IStringCompressor zipUtils, ILogger logger,
            Func<IGlobalInfoProvider, IPublicService> getDesignerService)
            : base(commandService, globalInfo, logger)
        {
            this.zipUtils = zipUtils;
            this.getDesignerService = getDesignerService;
        }

        private static IPublicService GetDesignerService(IGlobalInfoProvider globalInfoProvider)
        {
            return (PublicServiceClient) HttpContext.Current.Session[globalInfoProvider.GetCurrentUser().Name];
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
                    Items = list.Items.Select(x => new DesignerQuestionnaireListViewItem() {Id = x.Id, Title = x.Title}),
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
                RemoteFileInfo docSource =
                    this.DesignerService.DownloadQuestionnaire(new DownloadQuestionnaireRequest(request.QuestionnaireId));

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