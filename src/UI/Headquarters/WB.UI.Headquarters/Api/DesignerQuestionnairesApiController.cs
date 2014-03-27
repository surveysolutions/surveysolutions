using System;
using System.Linq;
using System.ServiceModel;
using System.Web.Http;
using Main.Core.Documents;
using Main.Core.Utility;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Headquarters.Authentication;
using WB.Core.BoundedContexts.Headquarters.Authentication.Models;
using WB.Core.BoundedContexts.Headquarters.Questionnaires;
using WB.Core.BoundedContexts.Headquarters.Questionnaires.Views;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernel.Utils.Compression;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;
using WB.UI.Headquarters.Models.Template;

namespace WB.UI.Headquarters.Api
{
    [Authorize(Roles = ApplicationRoles.Headquarter)]
    public class DesignerQuestionnairesApiController : ApiController
    {
        private readonly IStringCompressor zipUtils;
        private readonly ICommandService commandService;
        private readonly ILogger logger;
        private readonly ISupportedVersionProvider supportedVersionProvider;
        private readonly IDesignerService designerService;


        public DesignerQuestionnairesApiController(
            ISupportedVersionProvider supportedVersionProvider,
            IDesignerService designerService,
            IStringCompressor zipUtils,
            ICommandService commandService,
            ILogger logger)
        {
            this.zipUtils = zipUtils;
            this.commandService = commandService;
            this.logger = logger;
            this.supportedVersionProvider = supportedVersionProvider;
            this.designerService = designerService;
        }

        public DesignerQuestionnairesView QuestionnairesList(DesignerQuestionnairesListViewModel data)
        {
            var list =
                this.designerService.GetQuestionnaireList(data.Request.Filter, data.Pager.Page, data.Pager.PageSize, StringUtil.GetOrderRequestString(data.SortOrder));

            return new DesignerQuestionnairesView()
                {
                    Items = list.Items.Select(x => new DesignerQuestionnaireListViewItem { Id = x.Id, Title = x.Title }),
                    TotalCount = list.Total,
                    ItemsSummary = null
                };
        }

        [HttpPost]
        public QuestionnaireVerificationResponse GetQuestionnaire(ImportQuestionnaireRequest request)
        {
            QuestionnaireDocument document = null;
            try
            {
                QuestionnaireVersion supportedVerstion = this.supportedVersionProvider.GetSupportedQuestionnaireVersion();
                RemoteFileInfo docSource;
                try
                {
                    docSource = this.designerService.DownloadQuestionnaire(request.QuestionnaireId, new QuestionnaireVersion (
                                supportedVerstion.Major,
                                supportedVerstion.Minor,
                                supportedVerstion.Patch
                            ));
                }
                catch (FaultException ex)
                {
                    this.logger.Error(string.Format("Designer: error when importing template #{0}", request.QuestionnaireId), ex);
                    return new QuestionnaireVerificationResponse(true)
                    {
                        ImportError = ex.Reason.ToString()
                    };
                }

                document = this.zipUtils.Decompress<QuestionnaireDocument>(docSource.FileByteStream);

                Guid createdBy = Guid.Parse((User as ApplicationUser).Id);
                this.commandService.Execute(new ImportFromDesigner(createdBy, document));

                return new QuestionnaireVerificationResponse(true);
            }
            catch (Exception ex)
            {
                var domainEx = ex.GetSelfOrInnerAs<QuestionnaireException>();
                if (domainEx == null)
                {
                    this.logger.Error(string.Format("Designer: error when importing template #{0}", request.QuestionnaireId), ex);
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