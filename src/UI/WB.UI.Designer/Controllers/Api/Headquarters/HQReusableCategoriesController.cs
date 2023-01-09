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
    [Route("api/hq/categories")]
    public class HQReusableCategoriesController : ControllerBase
    {
        private readonly IReusableCategoriesService reusableCategoriesService;

        public HQReusableCategoriesController(IReusableCategoriesService reusableCategoriesService)
        {
            this.reusableCategoriesService = reusableCategoriesService;
        }

        [HttpGet]
        [Route("{id}/{categoryId}")]
        public IActionResult Get(Guid id, Guid categoryId)
        {
            var categories = this.reusableCategoriesService.GetCategoriesById(id, categoryId);
            if (categories == null) return NotFound();

            var result = JsonConvert.SerializeObject(categories, Formatting.None, new JsonSerializerSettings
            { 
                TypeNameHandling = TypeNameHandling.None
            });

            return Content(result, "application/json", Encoding.UTF8);
        }
    }
}
