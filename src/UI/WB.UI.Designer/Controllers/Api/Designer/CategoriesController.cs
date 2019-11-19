using Microsoft.AspNetCore.Mvc;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Questionnaire.Categories;

namespace WB.UI.Designer.Controllers.Api.Designer
{
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

            if (categoriesFile.ContentAsExcelFile == null) return NotFound();

            var categoriesName = string.IsNullOrEmpty(categoriesFile.CategoriesName)
                ? "New categories"
                : categoriesFile.CategoriesName;

            var filename = this.fileSystemAccessor.MakeValidFileName($"[{categoriesName}]{categoriesFile.QuestionnaireTitle}");

            return File(categoriesFile.ContentAsExcelFile, "application/vnd.oasis.opendocument.spreadsheet", $"{filename}.xlsx");
        }
    }
}
