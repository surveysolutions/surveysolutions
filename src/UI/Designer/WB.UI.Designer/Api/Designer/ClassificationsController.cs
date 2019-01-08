using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Filters;
using WB.Core.BoundedContexts.Designer.Classifications;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.Membership;
using WB.UI.Designer.Filters;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Designer.Api.Designer
{
    public class ClassificationsExceptionFilterAttribute : ExceptionFilterAttribute 
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            if (!(context.Exception is ClassificationException exception)) return;
            switch (exception.ErrorType)
            {
                case ClassificationExceptionType.Undefined:
                    context.Response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    break;
                case ClassificationExceptionType.NoAccess:
                    context.Response = new HttpResponseMessage(HttpStatusCode.Forbidden);
                    break;
            }
        }
    }

    [ApiNoCache]
    [Authorize]
    [RoutePrefix("api")]
    [CamelCase]
    [ClassificationsExceptionFilter]
    public class ClassificationsController : ApiController
    {
        private readonly IClassificationsStorage classificationsStorage;
        private readonly IMembershipUserService membershipUserService;

        public ClassificationsController(
            IClassificationsStorage classificationsStorage, 
            IMembershipUserService membershipUserService)
        {
            this.classificationsStorage = classificationsStorage;
            this.membershipUserService = membershipUserService;
        }

        [HttpGet]
        [Route("classifications")]
        [ResponseType(typeof(IEnumerable<Classification>))]
        public Task<IEnumerable<Classification>> GetClassifications(Guid? groupId)
        {
            return classificationsStorage.GetClassifications(groupId, membershipUserService.WebUser.UserId);
        }

        [HttpGet]
        [Route("groups")]
        public Task<IEnumerable<ClassificationGroup>> GetGroups()
        {
            return classificationsStorage.GetClassificationGroups(membershipUserService.WebUser.UserId);
        }

        [HttpGet]
        [Route("classifications/search")]
        public Task<ClassificationsSearchResult> Search([FromUri] ClassificationSearchQueryModel model)
        {
            return classificationsStorage.SearchAsync(model.Query, model.GroupId, model.PrivateOnly, membershipUserService.WebUser.UserId);
        }

        [HttpGet]
        [Route("classification/{id}/categories")]
        public Task<List<Category>> Categories([FromUri] Guid id)
        {
            return classificationsStorage.GetCategories(id);
        }

        [HttpPatch]
        [Route("classification/{id}")]
        public Task UpdateClassification(Classification classification)
        {
            return classificationsStorage.UpdateClassification(classification, membershipUserService.WebUser.UserId, membershipUserService.WebUser.IsAdmin);
        }

        [HttpPost]
        [Route("classification")]
        public Task CreateClassification(Classification classification)
        {
            return classificationsStorage.CreateClassification(classification, membershipUserService.WebUser.UserId);
        }

        [HttpDelete]
        [Route("classification/{id}")]
        public Task DeleteClassification([FromUri] Guid id)
        {
            return classificationsStorage.DeleteClassification(id, membershipUserService.WebUser.UserId, membershipUserService.WebUser.IsAdmin);
        }

        [HttpPatch]
        [Route("group/{id}")]
        public Task UpdateClassificationGroup([FromUri] Guid id, ClassificationGroup group)
        {
            return classificationsStorage.UpdateClassificationGroup(group, membershipUserService.WebUser.IsAdmin);
        }

        [HttpPost]
        [Route("group")]
        public Task CreateClassificationGroup(ClassificationGroup group)
        {
            return classificationsStorage.CreateClassificationGroup(group, membershipUserService.WebUser.IsAdmin);
        }

        [HttpDelete]
        [Route("group/{id}")]
        public Task DeleteClassificationGroup([FromUri] Guid id)
        {
            return classificationsStorage.DeleteClassificationGroup(id, membershipUserService.WebUser.IsAdmin);
        }

        [HttpPost]
        [Route("classification/{id}/categories")]
        public Task UpdateCategories([FromUri] Guid id, Category[] categories)
        {
            return classificationsStorage.UpdateCategories(id, categories, membershipUserService.WebUser.UserId, membershipUserService.WebUser.IsAdmin);
        }
    }

    public class ClassificationSearchQueryModel
    {
        public string Query { get; set; }
        public Guid? GroupId{ get; set; }
        public bool PrivateOnly { get; set; } = false;
    }
}
