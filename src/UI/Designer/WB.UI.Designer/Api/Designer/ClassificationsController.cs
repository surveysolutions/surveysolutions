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
    [Authorize]
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
        public async Task<IEnumerable<Classification>> GetClassifications(Guid? groupId)
        {
            return await classificationsStorage.GetClassifications(groupId);
        }

        [HttpGet]
        [Route("groups")]
        public async Task<IEnumerable<ClassificationGroup>> GetGroups()
        {
            return await classificationsStorage.GetClassificationGroups();
        }

        [HttpGet]
        [Route("classifications/search")]
        public async Task<ClassificationsSearchResult> Search([FromUri] SearchQueryModel model)
        {
            return await classificationsStorage.SearchAsync(model.Query, model.GroupId);
        }

        [HttpGet]
        [Route("classification/{id}/categories")]
        public async Task<IEnumerable<Category>> Categories([FromUri] Guid id)
        {
            return await classificationsStorage.GetCategories(id);
        }

        [HttpPatch]
        [Route("classification/{id}")]
        public async Task UpdateClassification(Classification classification)
        {
            await classificationsStorage.UpdateClassification(classification);
        }
        [HttpPost]
        [Route("classification")]
        public async Task CreateClassification(Classification classification)
        {
            await classificationsStorage.CreateClassification(classification);
        }

        [HttpDelete]
        [Route("classification/{id}")]
        public async Task DeleteClassification([FromUri] Guid id)
        {
            await classificationsStorage.DeleteClassification(id);
        }

        [HttpPatch]
        [Route("group/{id}")]
        public async Task UpdateClassificationGroup([FromUri] Guid id, ClassificationGroup group)
        {
            await classificationsStorage.UpdateClassificationGroup(group);
        }

        [HttpPost]
        [Route("group")]
        public async Task CreateClassificationGroup(ClassificationGroup group)
        {
            await classificationsStorage.CreateClassificationGroup(group);
        }

        [HttpDelete]
        [Route("group/{id}")]
        public async Task DeleteClassificationGroup([FromUri] Guid id)
        {
            await classificationsStorage.DeleteClassificationGroup(id);
        }

        [HttpPost]
        [Route("classification/{id}/categories")]
        public async Task UpdateCategories([FromUri] Guid id, Category[] categories)
        {
            await classificationsStorage.UpdateCategories(id, categories);
        }
    }

    public class SearchQueryModel
    {
        public string Query { get; set; }
        public Guid? GroupId{ get; set; }
    }
}
