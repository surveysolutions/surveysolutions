using System;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.UI.Designer.Resources;

namespace WB.UI.Designer.Controllers.Api.Designer
{
    [Route("categories")]
    [QuestionnairePermissions]
    [AuthorizeOrAnonymousQuestionnaire]
    public class CategoriesController : Controller
    {
        private readonly IReusableCategoriesService reusableCategoriesService;
        private readonly IFileSystemAccessor fileSystemAccessor;

        public CategoriesController(IReusableCategoriesService reusableCategoriesService, IFileSystemAccessor fileSystemAccessor)
        {
            this.reusableCategoriesService = reusableCategoriesService;
            this.fileSystemAccessor = fileSystemAccessor;
        }

        [HttpGet]
        [Route("template")]
        public IActionResult Get()
        {
            var categoriesFile = this.reusableCategoriesService.GetTemplate(CategoriesFileType.Excel);

            if (categoriesFile == null) return NotFound();

            return File(categoriesFile, "application/vnd.oasis.opendocument.spreadsheet",
                $"{QuestionnaireEditor.SideBarCategoriesTitle}.xlsx");
        }

        [HttpGet]
        [Route("templateCsv")]
        public IActionResult GetCsv()
        {
            var categoriesFile = this.reusableCategoriesService.GetTemplate(CategoriesFileType.Tsv);

            if (categoriesFile == null) 
                return NotFound();

            return File(categoriesFile, "text/plain", $"{QuestionnaireEditor.SideBarCategoriesTitle}.txt");
        }

        [HttpGet]
        [Route("{id}/xlsx/{categoriesId:Guid}")]
        public IActionResult Get(QuestionnaireRevision id, Guid categoriesId)
        {
            var categoriesFile = this.reusableCategoriesService.GetAsFile(id, categoriesId, CategoriesFileType.Excel);

            if (categoriesFile?.Content == null) return NotFound();

            var categoriesName = string.IsNullOrEmpty(categoriesFile.CategoriesName)
                ? "New categories"
                : categoriesFile.CategoriesName;

            var filename = this.fileSystemAccessor.MakeValidFileName($"[{categoriesName}]{categoriesFile.QuestionnaireTitle}");

            return File(categoriesFile.Content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{filename}.xlsx");
        }
    }
}
