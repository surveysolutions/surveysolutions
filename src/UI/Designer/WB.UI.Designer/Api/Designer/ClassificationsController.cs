using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Filters;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Designer.Classifications;
using WB.Core.BoundedContexts.Designer.Exceptions;
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

        public ClassificationsController(IClassificationsStorage classificationsStorage)
        {
            this.classificationsStorage = classificationsStorage;
        }

        [HttpPost]
        [Route("init")]
        [Authorize(Roles = "Administrator")]
        public void Init()
        {
            var json = File.ReadAllText(HostingEnvironment.MapPath("~/Content/QbankClassifications.json"));
            var entities = JsonConvert.DeserializeObject<MysqlClassificationEntity[]>(json);
            foreach (var entity in entities)
            {
                entity.IdGuid = Guid.NewGuid();
            }

            foreach (var entity in entities)
            {
                if (!entity.Parent.HasValue) continue;
                switch (entity.Type)
                {
                    case ClassificationEntityType.Group:
                        break;
                    case ClassificationEntityType.Classification:
                        var parenGroup = entities.FirstOrDefault(x => x.Id == entity.Parent.Value && x.Type == ClassificationEntityType.Group);
                        entity.ParentGuid = parenGroup?.IdGuid;
                        entity.ClassificationId = entity.IdGuid;
                        break;
                    case ClassificationEntityType.Category:
                        var parenClassification = entities.FirstOrDefault(x => x.Id == entity.Parent.Value && x.Type == ClassificationEntityType.Classification);
                        entity.ParentGuid = parenClassification?.IdGuid;
                        entity.ClassificationId = entity.ParentGuid;
                        break;
                }
            }

            var bdEntities = entities.Select(x => new ClassificationEntity
            {
                Id = x.IdGuid,
                Value = x.Value,
                Title = x.Title,
                Type = x.Type,
                Index = x.Order,
                Parent = x.ParentGuid,
                ClassificationId = x.ClassificationId
            }).ToArray();

            classificationsStorage.Store(bdEntities);
        }

        [HttpGet]
        [Route("classifications")]
        [ResponseType(typeof(IEnumerable<Classification>))]
        public Task<IEnumerable<Classification>> GetClassifications(Guid? groupId)
        {
            return classificationsStorage.GetClassifications(groupId);
        }

        [HttpGet]
        [Route("groups")]
        public Task<IEnumerable<ClassificationGroup>> GetGroups()
        {
            return classificationsStorage.GetClassificationGroups();
        }

        [HttpGet]
        [Route("classifications/search")]
        public Task<ClassificationsSearchResult> Search([FromUri] ClassificationSearchQueryModel model)
        {
            return classificationsStorage.SearchAsync(model.Query, model.GroupId, model.PrivateOnly);
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
            return classificationsStorage.UpdateClassification(classification);
        }

        [HttpPost]
        [Route("classification")]
        public Task CreateClassification(Classification classification)
        {
            return classificationsStorage.CreateClassification(classification);
        }

        [HttpDelete]
        [Route("classification/{id}")]
        public Task DeleteClassification([FromUri] Guid id)
        {
            return classificationsStorage.DeleteClassification(id);
        }

        [HttpPatch]
        [Route("group/{id}")]
        public Task UpdateClassificationGroup([FromUri] Guid id, ClassificationGroup group)
        {
            return classificationsStorage.UpdateClassificationGroup(group);
        }

        [HttpPost]
        [Route("group")]
        public Task CreateClassificationGroup(ClassificationGroup group)
        {
            return classificationsStorage.CreateClassificationGroup(group);
        }

        [HttpDelete]
        [Route("group/{id}")]
        public Task DeleteClassificationGroup([FromUri] Guid id)
        {
            return classificationsStorage.DeleteClassificationGroup(id);
        }

        [HttpPost]
        [Route("classification/{id}/categories")]
        public Task UpdateCategories([FromUri] Guid id, Category[] categories)
        {
            return classificationsStorage.UpdateCategories(id, categories);
        }
    }

    public class ClassificationSearchQueryModel
    {
        public string Query { get; set; }
        public Guid? GroupId{ get; set; }
        public bool PrivateOnly { get; set; } = false;
    }
}
