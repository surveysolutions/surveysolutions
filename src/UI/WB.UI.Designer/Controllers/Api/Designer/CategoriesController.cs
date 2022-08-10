using System;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.UI.Designer.Resources;

namespace WB.UI.Designer.Controllers.Api.Designer
{
    [AuthorizeOrAnonymousQuestionnaire]
    [QuestionnairePermissions]
    [Route("categories")]
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
            var categoriesFile = this.categoriesService.GetTemplateAsExcelFile();

            if (categoriesFile == null) return NotFound();

            return File(categoriesFile, "application/vnd.oasis.opendocument.spreadsheet",
                $"{QuestionnaireEditor.SideBarCategoriesTitle}.xlsx");
        }

        [HttpGet]
        [Route("{id:Guid}/xlsx/{categoriesId:Guid}")]
        public IActionResult Get(QuestionnaireRevision id, Guid categoriesId)
        {
            var categoriesFile = this.categoriesService.GetAsExcelFile(id, categoriesId);

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
