// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PublicService.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace WB.UI.Designer.WebServices
{
    using System;
    using System.IO;
    using System.Web.Security;

    using WB.Core.Questionnaire.ExportServices;
    using WB.UI.Desiner.Utilities.Compression;

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
        public PublicService(IExportService exportService, IZipUtils zipUtils)
        {
            this.exportService = exportService;
            this.zipUtils = zipUtils;
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

        #endregion
    }
}