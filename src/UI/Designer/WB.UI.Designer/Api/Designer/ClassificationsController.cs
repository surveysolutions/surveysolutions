using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Designer.Classifications;
using WB.UI.Designer.Filters;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Designer.Api.Designer
{
    [Authorize]
    [ApiNoCache]
    [Authorize]
    [RoutePrefix("api")]
    [CamelCase]
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
            var json = File.ReadAllText(@"D:\Temp\aaa.json");
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
                        break;
                    case ClassificationEntityType.Category:
                        var parenClassification = entities.FirstOrDefault(x => x.Id == entity.Parent.Value && x.Type == ClassificationEntityType.Classification);
                        entity.ParentGuid = parenClassification?.IdGuid;
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
                Parent = x.ParentGuid
            }).ToArray();

            classificationsStorage.Store(bdEntities);
        }

        [HttpGet]
        [Route("classifications")]
        public async Task<IEnumerable<Classification>> GetClassifications()
        {
            return await classificationsStorage.GetClassifications();
        }

        [HttpGet]
        [Route("classifications/groups")]
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
    }

    public class SearchQueryModel
    {
        public string Query { get; set; }
        public Guid? GroupId{ get; set; }
    }
}
