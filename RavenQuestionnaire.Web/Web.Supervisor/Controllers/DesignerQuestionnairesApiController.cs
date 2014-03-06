﻿using System;
using System.Linq;
using System.Web;
using System.Web.Http;
using Core.Supervisor.Views.Template;
using Main.Core.Documents;
using Main.Core.Utility;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using WB.Core.BoundedContexts.Supervisor.Services;
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
            get { return this.DesignerServiceClient; }
        }

        private PublicServiceClient DesignerServiceClient
        {
            get { return (PublicServiceClient)HttpContext.Current.Session[this.GlobalInfo.GetCurrentUser().Name]; }
            set { HttpContext.Current.Session[this.GlobalInfo.GetCurrentUser().Name] = value; }
        }

        private readonly IStringCompressor zipUtils;
        private readonly ISupportedVersionProvider supportedVersionProvider;

        public DesignerQuestionnairesApiController(ICommandService commandService, IGlobalInfoProvider globalInfo,
            ISupportedVersionProvider supportedVersionProvider,
            IStringCompressor zipUtils, ILogger logger)
            : base(commandService, globalInfo, logger)
        {
            this.zipUtils = zipUtils;
            this.supportedVersionProvider = supportedVersionProvider;
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
                var supportedVerstion = supportedVersionProvider.GetSupportedQuestionnaireVersion();
                RemoteFileInfo docSource = this.DesignerService.DownloadQuestionnaire(new DownloadQuestionnaireRequest(request.QuestionnaireId,
                        new QuestionnaireVersion()
                        {
                            Major = supportedVerstion.Major,
                            Minor = supportedVerstion.Minor,
                            Patch = supportedVerstion.Patch
                        }));

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
                    return new QuestionnaireVerificationResponse(false);
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