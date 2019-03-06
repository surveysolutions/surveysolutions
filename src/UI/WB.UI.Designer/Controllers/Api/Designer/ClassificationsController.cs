using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WB.Core.BoundedContexts.Designer.Classifications;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.UI.Designer1.Extensions;

namespace WB.UI.Designer.Api.Designer
{
    public class ClassificationsExceptionFilterAttribute : ExceptionFilterAttribute 
    {
        public override void OnException(ExceptionContext context)
        {
            if (!(context.Exception is ClassificationException exception)) return;
            switch (exception.ErrorType)
            {
                case ClassificationExceptionType.Undefined:
                    context.Result = new BadRequestResult();;
                    break;
                case ClassificationExceptionType.NoAccess:
                    context.Result = new ForbidResult();
                    break;
            }
        }
    }

    [ResponseCache(NoStore = true)]
    [Authorize]
    [Route("api")]
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
        public Task<List<Category>> Categories([FromQueryAttribute] Guid id)
        {
            return classificationsStorage.GetCategories(id);
        }

        [HttpPatch]
        [Route("classification/{id}")]
        public Task UpdateClassification(Classification classification)
        {
            return classificationsStorage.UpdateClassification(classification, User.GetId(), User.IsAdmin());
        }

        [HttpPost]
        [Route("classification")]
        public Task CreateClassification(Classification classification)
        {
            return classificationsStorage.CreateClassification(classification, User.GetId());
        }

        [HttpDelete]
        [Route("classification/{id}")]
        public Task DeleteClassification([FromQueryAttribute] Guid id)
        {
            return classificationsStorage.DeleteClassification(id, User.GetId(), User.IsAdmin());
        }

        [HttpPatch]
        [Route("group/{id}")]
        public Task UpdateClassificationGroup([FromQueryAttribute] Guid id, ClassificationGroup group)
        {
            return classificationsStorage.UpdateClassificationGroup(group, User.IsAdmin());
        }

        [HttpPost]
        [Route("group")]
        public Task CreateClassificationGroup(ClassificationGroup group)
        {
            return classificationsStorage.CreateClassificationGroup(group, User.IsAdmin());
        }

        [HttpDelete]
        [Route("group/{id}")]
        public Task DeleteClassificationGroup([FromQueryAttribute] Guid id)
        {
            return classificationsStorage.DeleteClassificationGroup(id, User.IsAdmin());
        }

        [HttpPost]
        [Route("classification/{id}/categories")]
        public Task UpdateCategories([FromQueryAttribute] Guid id, Category[] categories)
        {
            return classificationsStorage.UpdateCategories(id, categories, User.GetId(), User.IsAdmin());
        }
    }

    public class ClassificationSearchQueryModel
    {
        public string Query { get; set; }
        public Guid? GroupId{ get; set; }
        public bool PrivateOnly { get; set; } = false;
    }
}
