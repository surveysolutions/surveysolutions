using WB.Core.BoundedContexts.Designer.Services;
using WB.UI.Designer.Views.Questionnaire;

namespace WB.UI.Designer.WebServices
{
    using System;
    using System.IO;

    using Main.Core.View;
    using WB.Core.SharedKernel.Utils.Compression;
    using WB.UI.Designer.WebServices.Questionnaire;
    using WB.UI.Shared.Web;
    using WB.UI.Shared.Web.Membership;

    public class PublicService : IPublicService
    {
        private readonly IJsonExportService exportService;
        private readonly IMembershipUserService userHelper;
        private readonly IStringCompressor zipUtils;
        private readonly IViewFactory<QuestionnaireListViewInputModel, QuestionnaireListView> viewFactory;

        public PublicService(
            IJsonExportService exportService,
            IStringCompressor zipUtils, 
            IMembershipUserService userHelper,
            IViewFactory<QuestionnaireListViewInputModel, WB.UI.Designer.Views.Questionnaire.QuestionnaireListView> viewFactory)
        {
            this.exportService = exportService;
            this.zipUtils = zipUtils;
            this.userHelper = userHelper;
            this.viewFactory = viewFactory;
        }

        public RemoteFileInfo DownloadQuestionnaire(DownloadQuestionnaireRequest request)
        {
            var templateInfo = this.exportService.GetQuestionnaireTemplate(request.QuestionnaireId);

            if (templateInfo == null || string.IsNullOrEmpty(templateInfo.Source))
            {
                return null;
            }

            Stream stream = this.zipUtils.Compress(templateInfo.Source);

            return new RemoteFileInfo
                       {
                           FileName = string.Format("{0}.tmpl", templateInfo.Title.ToValidFileName()),
                           Length = stream.Length,
                           FileByteStream = stream
                       };
        }

        public string DownloadQuestionnaireSource(Guid request)
        {
            var templateInfo = this.exportService.GetQuestionnaireTemplate(request);
            return templateInfo == null ? string.Empty : templateInfo.Source;
        }

        public void Dummy()
        {
        }

        public QuestionnaireListViewMessage GetQuestionnaireList(QuestionnaireListRequest request)
        {
            return new QuestionnaireListViewMessage(
                this.viewFactory.Load(
                    input:
                        new QuestionnaireListViewInputModel
                            {

                                ViewerId = this.userHelper.WebServiceUser.UserId,
                                IsAdminMode = this.userHelper.WebServiceUser.IsAdmin,
                                Page = request.PageIndex,
                                PageSize = request.PageSize,
                                Order = request.SortOrder,
                                Filter = request.Filter
                            }));
        }
    }
}