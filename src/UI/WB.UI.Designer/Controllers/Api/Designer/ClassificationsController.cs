using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Designer.Classifications;
using WB.UI.Designer.Extensions;
using WB.Core.BoundedContexts.Designer;

namespace WB.UI.Designer.Controllers.Api.Designer
{
    [ResponseCache(NoStore = true)]
    [Authorize]
    [Route("api/classifications")]
    [ClassificationsExceptionFilter]
    public class ClassificationsController : Controller
    {
        private readonly IClassificationsStorage classificationsStorage;

        public ClassificationsController(
            IClassificationsStorage classificationsStorage)
        {
            this.classificationsStorage = classificationsStorage;
        }

        [HttpGet]
        [Route("user")]
        public IActionResult UserInfo(Guid groupId)
        {
            return new JsonResult(new { userId = User.GetId(), isAdmin = User.IsAdmin() });
        }
        
        [HttpGet]
        [Route("classifications")]
        public Task<IEnumerable<Classification>> GetClassifications(Guid groupId)
        {
            return classificationsStorage.GetClassifications(groupId, User.GetId());
        }

        [HttpGet]
        [Route("groups")]
        public Task<IEnumerable<ClassificationGroup>> GetGroups()
        {
            return classificationsStorage.GetClassificationGroups(User.GetId());
        }

        [HttpGet]
        [Route("classifications/search")]
        public Task<ClassificationsSearchResult> Search([FromQuery] ClassificationSearchQueryModel model)
        {
            return classificationsStorage.SearchAsync(model.Query, model.GroupId, model.PrivateOnly, User.GetId());
        }

        [HttpGet]
        [Route("classification/{id}/categories")]
        public Task<List<Category>> Categories(Guid id)
        {
            return classificationsStorage.GetCategories(id);
        }

        [HttpPatch]
        [Route("classification/{id}")]
        public Task UpdateClassification([FromBody] Classification classification)
        {
            return classificationsStorage.UpdateClassification(classification, User.GetId(), User.IsAdmin());
        }

        [HttpPost]
        [Route("classification")]
        public Task CreateClassification([FromBody] Classification classification)
        {
            return classificationsStorage.CreateClassification(classification, User.GetId());
        }

        [HttpDelete]
        [Route("classification/{id}")]
        public Task DeleteClassification(Guid id)
        {
            return classificationsStorage.DeleteClassificationAsync(id, User.GetId(), User.IsAdmin());
        }

        [HttpPatch]
        [Route("group/{id}")]
        public Task UpdateClassificationGroup(Guid id, [FromBody] ClassificationGroup group)
        {
            return classificationsStorage.UpdateClassificationGroup(group, User.IsAdmin());
        }

        [HttpPost]
        [Route("group")]
        public Task CreateClassificationGroup([FromBody] ClassificationGroup group)
        {
            return classificationsStorage.CreateClassificationGroup(group, User.IsAdmin());
        }

        [HttpDelete]
        [Route("group/{id}")]
        public Task DeleteClassificationGroup(Guid id)
        {
            return classificationsStorage.DeleteClassificationGroup(id, User.IsAdmin());
        }

        [HttpPost]
        [Route("classification/{id}/categories")]
        public Task UpdateCategories(Guid id, [FromBody] Category[] categories)
        {
            return classificationsStorage.UpdateCategories(id, categories, User.GetId(), User.IsAdmin());
        }
    }

    public class ClassificationSearchQueryModel
    {
        public string? Query { get; set; }
        public Guid? GroupId{ get; set; }
        public bool PrivateOnly { get; set; } = false;
    }
}
