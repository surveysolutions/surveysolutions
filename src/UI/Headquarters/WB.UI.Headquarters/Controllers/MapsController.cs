using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Controllers
{
    [LimitsFilter]
    [Authorize(Roles = "Administrator, Headquarter")]
    public class MapsController : BaseController
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IMapRepository mapRepository;

        public ActionResult Index()
        {
            this.ViewBag.ActivePage = MenuItem.Maps;
            return View();
        }

        public MapsController(ICommandService commandService, ILogger logger, 
            IFileSystemAccessor fileSystemAccessor, IMapRepository mapRepository) : base(commandService, logger)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.mapRepository = mapRepository;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
        public ActionResult UploadMaps(MapsUploadModel model)
        {
            this.ViewBag.ActivePage = MenuItem.Questionnaires;

            if (!this.ModelState.IsValid)
            {
                return this.RedirectToAction(nameof(Index));
            }
            
            if (".zip" != this.fileSystemAccessor.GetFileExtension(model.File.FileName).ToLower())
            {
                return this.RedirectToAction(nameof(Index));
            }

            string tempStore = null;
            var invalidMaps = new List<string>();
            bool genericErrorOccurred = false;
            try
            {
                tempStore = mapRepository.StoreData(model.File.InputStream, model.File.FileName);
                var maps = mapRepository.UnzipAndGetFileList(tempStore);
                
                foreach (var map in maps)
                {
                    try
                    {
                        mapRepository.SaveOrUpdateMap(map);
                    }
                    catch (Exception e)
                    {
                        Logger.Error($"Error on maps import map {map}", e);
                        invalidMaps.Add(map);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error("Error on maps import", e);
                genericErrorOccurred = true;
            }
            finally
            {
                if(tempStore!=null)
                    mapRepository.DeleteTempData(tempStore);
            }

            if (genericErrorOccurred || invalidMaps.Count > 0)
            {

            }

            return this.RedirectToAction("Index");
        }
    }

    public class MapsUploadModel
    {
        [Required(ErrorMessageResourceType = typeof(FieldsAndValidations), ErrorMessageResourceName = nameof(FieldsAndValidations.BatchUploadModel_Required))]
        [ValidateFile(ErrorMessageResourceType = typeof(FieldsAndValidations), ErrorMessageResourceName = nameof(FieldsAndValidations.BatchUploadModel_ValidationErrorMessage))]
        [Display(ResourceType = typeof(FieldsAndValidations), Name = nameof(FieldsAndValidations.BatchUploadModel_FileName))]
        public HttpPostedFileBase File { get; set; }
        
        public int ClientTimezoneOffset { get; set; }
    }
}