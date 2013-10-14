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
using Web.Supervisor.DesignerPublicService;
using Web.Supervisor.Models;

namespace Web.Supervisor.Controllers
{
    [Authorize(Roles = "Headquarter")]
    public class DesignerQuestionnairesApiController : BaseApiController
    {
        private IPublicService DesignerService
        {
            get { return this.DesignerServiceClient; }
        }

        private PublicServiceClient DesignerServiceClient
        {
            get { return (PublicServiceClient)HttpContext.Current.Session[this.GlobalInfo.GetCurrentUser().Name]; }
            set { HttpContext.Current.Session[this.GlobalInfo.GetCurrentUser().Name] = value; }
        }

        private readonly IStringCompressor zipUtils;

        public DesignerQuestionnairesApiController(ICommandService commandService, IGlobalInfoProvider globalInfo,
                                                   IStringCompressor zipUtils, ILogger logger)
            : base(commandService, globalInfo, logger)
        {
            this.zipUtils = zipUtils;
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
        public JsonBaseResponse GetQuestionnaire(ImportQuestionnaireRequest request)
        {
            var returnedJson = new JsonBaseResponse();

            try
            {
                RemoteFileInfo docSource =
                    this.DesignerService.DownloadQuestionnaire(new DownloadQuestionnaireRequest(request.QuestionnaireId));
                var document = this.zipUtils.Decompress<QuestionnaireDocument>(docSource.FileByteStream);

                this.CommandService.Execute(new ImportQuestionnaireCommand(this.GlobalInfo.GetCurrentUser().Id, document));

                returnedJson.IsSuccess = true;
            }
            catch (Exception ex)
            {
                this.Logger.Error(
                    string.Format("Designer: error when importing template #{0}", request.QuestionnaireId), ex);
            }

            return returnedJson;
        }

        public class ImportQuestionnaireRequest
        {
            public Guid QuestionnaireId { get; set; }
        }
    }
}