using WB.Core.BoundedContexts.Designer.Services;

namespace WB.UI.Designer.WebServices
{
    using System;
    using System.IO;

    using Main.Core.View;
    using WB.Core.SharedKernel.Utils.Compression;
    using WB.UI.Designer.WebServices.Questionnaire;
    using WB.UI.Shared.Web.Membership;

    public class PublicService : IPublicService
    {
        private readonly IExportService exportService;
        private readonly IMembershipUserService userHelper;
        private readonly IStringCompressor zipUtils;
        private readonly IViewFactory<QuestionnaireListViewInputModel, QuestionnaireListView> viewFactory;

        public PublicService(
            IExportService exportService,
            IStringCompressor zipUtils, 
            IMembershipUserService userHelper,
            IViewFactory<QuestionnaireListViewInputModel, QuestionnaireListView> viewFactory)
        {
            this.exportService = exportService;
            this.zipUtils = zipUtils;
            this.userHelper = userHelper;
            this.viewFactory = viewFactory;
        }

        public RemoteFileInfo DownloadQuestionnaire(DownloadQuestionnaireRequest request)
        {
            string data = this.exportService.GetQuestionnaireTemplate(request.QuestionnaireId);

            if (string.IsNullOrEmpty(data))
            {
                return null;
            }

            Stream stream = this.zipUtils.Compress(data);

            return new RemoteFileInfo { FileName = "template.zip", Length = stream.Length, FileByteStream = stream };
        }

        public string DownloadQuestionnaireSource(Guid request)
        {
            return this.exportService.GetQuestionnaireTemplate(request);
        }

        public void Dummy()
        {
        }

        public QuestionnaireListView GetQuestionnaireList(QuestionnaireListRequest request)
        {
            return
                this.viewFactory.Load(
                    input:
                        new QuestionnaireListViewInputModel
                            {
                                CreatedBy = this.userHelper.WebServiceUser.UserId, 
                                IsAdmin = this.userHelper.WebServiceUser.IsAdmin, 
                                Page = request.PageIndex, 
                                PageSize = request.PageSize, 
                                Order = request.SortOrder, 
                                Filter = request.Filter
                            });
        }
    }
}