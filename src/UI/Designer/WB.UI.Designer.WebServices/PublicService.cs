// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PublicService.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace WB.UI.Designer.WebServices
{
    using System;
    using System.IO;

    using Main.Core.View;

    using WB.Core.Questionnaire.ExportServices;
    using WB.UI.Designer.WebServices.Questionnaire;
    using WB.UI.Shared.Compression;
    using WB.UI.Shared.Web.Membership;

    /// <summary>
    ///     The public service.
    /// </summary>
    public class PublicService : IPublicService
    {
        #region Fields

        /// <summary>
        ///     The export service.
        /// </summary>
        private readonly IExportService exportService;

        /// <summary>
        ///     The repository.
        /// </summary>
        private readonly IViewRepository repository;

        /// <summary>
        ///     The user helper.
        /// </summary>
        private readonly IMembershipUserService userHelper;

        /// <summary>
        ///     The zip utils.
        /// </summary>
        private readonly IZipUtils zipUtils;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PublicService"/> class.
        /// </summary>
        /// <param name="exportService">
        /// The export service.
        /// </param>
        /// <param name="zipUtils">
        /// The zip utils.
        /// </param>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="userHelper">
        /// The user helper.
        /// </param>
        public PublicService(
            IExportService exportService, 
            IZipUtils zipUtils, 
            IViewRepository repository, 
            IMembershipUserService userHelper)
        {
            this.exportService = exportService;
            this.zipUtils = zipUtils;
            this.repository = repository;
            this.userHelper = userHelper;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The download questionnaire.
        /// </summary>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <returns>
        /// The <see cref="RemoteFileInfo"/>.
        /// </returns>
        public RemoteFileInfo DownloadQuestionnaire(DownloadQuestionnaireRequest request)
        {
            string data = this.exportService.GetQuestionnaireTemplate(request.QuestionnaireId);

            if (string.IsNullOrEmpty(data))
            {
                return null;
            }

            Stream stream = this.zipUtils.Zip(data);

            return new RemoteFileInfo { FileName = "template.zip", Length = stream.Length, FileByteStream = stream };
        }

        /// <summary>
        /// The download questionnaire source.
        /// </summary>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string DownloadQuestionnaireSource(Guid request)
        {
            return this.exportService.GetQuestionnaireTemplate(request);
        }

        /// <summary>
        /// The dummy.
        /// </summary>
        public void Dummy()
        {
        }

        /// <summary>
        /// The get questionnaire list.
        /// </summary>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <returns>
        /// The <see cref="QuestionnaireListView"/>.
        /// </returns>
        public QuestionnaireListView GetQuestionnaireList(QuestionnaireListRequest request)
        {
            return
                this.repository.Load<QuestionnaireListViewInputModel, QuestionnaireListView>(
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

        #endregion
    }
}