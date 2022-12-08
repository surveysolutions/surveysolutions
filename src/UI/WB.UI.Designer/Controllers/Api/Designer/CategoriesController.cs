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
        private readonly ICategoriesService categoriesService;
        private readonly IFileSystemAccessor fileSystemAccessor;

        public CategoriesController(ICategoriesService categoriesService, IFileSystemAccessor fileSystemAccessor)
        {
            this.categoriesService = categoriesService;
            this.fileSystemAccessor = fileSystemAccessor;
        }

        [HttpGet]
        [Route("template")]
        public IActionResult Get()
        {
            var categoriesFile = this.categoriesService.GetTemplate(CategoriesFileType.Excel);

            if (categoriesFile == null) return NotFound();

            return File(categoriesFile, "application/vnd.oasis.opendocument.spreadsheet",
                $"{QuestionnaireEditor.SideBarCategoriesTitle}.xlsx");
        }

        [HttpGet]
        [Route("templateCsv")]
        public IActionResult GetCsv()
        {
            var categoriesFile = this.categoriesService.GetTemplate(CategoriesFileType.Tsv);

            if (categoriesFile == null) 
                return NotFound();

            return File(categoriesFile, "text/plain", $"{QuestionnaireEditor.SideBarCategoriesTitle}.txt");
        }

        [HttpGet]
        [Route("{id}/xlsx/{categoriesId:Guid}")]
        public IActionResult Get(QuestionnaireRevision id, Guid categoriesId)
        {
            var categoriesFile = this.categoriesService.GetAsFile(id, categoriesId, CategoriesFileType.Excel);

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
