using System;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.UI.Designer.Code.Attributes;

namespace WB.UI.Designer.Controllers.Api.Headquarters
{
    [Authorize]
    [AllowOnlyFromWhitelistIP]
    [Route("api/hq/categories")]
    public class HQReusableCategoriesController : ControllerBase
    {
        private readonly ICategoriesService categoriesService;

        public HQReusableCategoriesController(ICategoriesService categoriesService)
        {
            this.categoriesService = categoriesService;
        }

        [HttpGet]
        [Route("{id}/{categoryId}")]
        public IActionResult Get(string id, string categoryId)
        {
            var contentFile = this.categoriesService.GetPlainContentFile(Guid.Parse(id), Guid.Parse(categoryId));

            if (contentFile == null) return NotFound();

            var result = JsonConvert.SerializeObject(contentFile, Formatting.None, new JsonSerializerSettings
            { 
                TypeNameHandling = TypeNameHandling.None
            });

            return Content(result, "application/json", Encoding.UTF8);
        }
    }
}
