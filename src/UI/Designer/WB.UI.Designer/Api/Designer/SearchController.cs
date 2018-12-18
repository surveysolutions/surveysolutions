using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.UI.Designer.Filters;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Designer.Api.Designer
{
    [ApiNoCache]
    [Authorize]
    [RoutePrefix("api/search")]
    [CamelCase]
    public class SearchController : ApiController
    {
        private readonly IPublicFoldersStorage publicFoldersStorage;

        public SearchController(IPublicFoldersStorage publicFoldersStorage)
        {
            this.publicFoldersStorage = publicFoldersStorage;
        }

        [HttpGet]
        [Route("filters")]
        public Task<List<QuestionnaireListViewFolder>> GetFilters()
        {
            return publicFoldersStorage.GetAllFolders();
        }

        [HttpGet]
        [Route("")]
        public Task<SearchResultModel> Search([FromUri] SearchQueryModel model)
        {
            var result = new SearchResultModel
            {
                Total = 348,
                Entities = new List<QuestionnaireSearchResultEntity>
                {
                    new QuestionnaireSearchResultEntity
                    {
                        Title = $"Religion of head ${model.Query}",
                        QuestionnaireTitle = "Identifying questions",
                        ItemType = ChapterItemType.Question.ToString(),
                        QuestionnaireId = "ce46512192894cf1b821418124a40fa2",
                        SectionId="22d8dd075d454d22a2219e0a21832d8c",
                        ItemId = "38bb7ed290ab6896ccfcb44081a05aca",
                        Folder = new QuestionnaireListViewFolder
                        {
                            Title = "UN Food and Agriculture Organization",
                            PublicId = Guid.NewGuid()
                        }
                    }
                }
            };
            return Task.FromResult(result);
        }
    }

    public class SearchResultModel
    {
        public List<QuestionnaireSearchResultEntity> Entities { get; set; }
        public int Total { get; set; }
    }

    public class QuestionnaireSearchResultEntity
    {
        public string Title { get; set; }
        public string QuestionnaireTitle { get; set; }
        public QuestionnaireListViewFolder Folder { get; set; }
        public string QuestionnaireId { get; set; }
        public string SectionId { get; set; }
        public string  ItemId  { get; set; }
        public string ItemType { get; set; }
    }

    public class SearchQueryModel
    {
        public string Query { get; set; }
        public Guid? FolderId{ get; set; }
        public bool PrivateOnly { get; set; } = false;
    }
}
