// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImportExportController.cs" company="World bank">
//   2012
// </copyright>
// <summary>
//   The import export controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using RavenQuestionnaire.Web.Export;

namespace RavenQuestionnaire.Web.Controllers
{
    using System;
    using System.Web;
    using System.Web.Mvc;

    using Questionnaire.Core.Web.Export;
    using Questionnaire.Core.Web.Threading;

    /// <summary>
    /// The import export controller.
    /// </summary>
    public class ImportExportController : AsyncController
    {
        #region Constants and Fields

        /// <summary>
        /// The exportimport events.
        /// </summary>
        private readonly IExportImport exportimportEvents;

        /// <summary>
        /// exporter templates
        /// </summary>
        private readonly ITemplateExporter exporterTemplates;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportExportController"/> class.
        /// </summary>
        /// <param name="exportImport">
        /// The export import.
        /// </param>
        /// <param name="exporterTemplates">
        /// The exporter Templates.
        /// </param>
        public ImportExportController(IExportImport exportImport, ITemplateExporter exporterTemplates)
        {
            this.exportimportEvents = exportImport;
            this.exporterTemplates = exporterTemplates;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The export async.
        /// </summary>
        /// <param name="clientGuid">
        /// The client guid.
        /// </param>
        public void ExportAsync(Guid clientGuid)
        {
            AsyncQuestionnaireUpdater.Update(AsyncManager,
                () =>
                    {
                        try
                        {
                            this.AsyncManager.Parameters["result"] = this.exportimportEvents.Export(clientGuid);
                        }
                        catch
                        {
                            this.AsyncManager.Parameters["result"] = null;
                        }

                    });
        }

        /// <summary>
        /// The export completed.
        /// </summary>
        /// <param name="result">
        /// The result.
        /// </param>
        /// <returns>
        /// </returns>
        public ActionResult ExportCompleted(byte[] result)
        {
            return File(
                result, "application/zip", string.Format("backup-{0}.zip", DateTime.Now.ToString().Replace(" ", "_")));
        }

        /// <summary>
        /// The import.
        /// </summary>
        /// <returns>
        /// </returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Import()
        {
            return this.View("ViewTestUploadFile");
        }

        /// <summary>
        /// The import async.
        /// </summary>
        /// <param name="myfile">
        /// The myfile.
        /// </param>
        [AcceptVerbs(HttpVerbs.Post)]
        public void ImportAsync(HttpPostedFileBase myfile)
        {
            if (myfile != null && myfile.ContentLength != 0)
            {
                AsyncQuestionnaireUpdater.Update(AsyncManager,
                    () => this.exportimportEvents.Import(myfile));
            }
        }

        /// <summary>
        /// The import completed.
        /// </summary>
        /// <returns>
        /// </returns>
        public ActionResult ImportCompleted()
        {
            return this.RedirectToAction("Index", "Questionnaire");
        }

        /// <summary>
        /// Started download templates
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        public void ExportTemplatesAsync(Guid? id, Guid? clientGuid)
        {
            AsyncQuestionnaireUpdater.Update(AsyncManager,() =>
            {
                try
                {
                    AsyncManager.Parameters["result"] = exporterTemplates.ExportTemplate(id, clientGuid);
                }
                catch
                {
                    AsyncManager.Parameters["result"] = null;
                }
            });
        }

        /// <summary>
        /// finish download template
        /// </summary>
        /// <param name="result">
        /// The result.
        /// </param>
        /// <returns>
        /// return file with templates
        /// </returns>
        public FileResult ExportTemplatesCompleted(byte[] result)
        {
            return this.File(result, "application/zip", "template.zip");
        }


        #endregion
    }
}