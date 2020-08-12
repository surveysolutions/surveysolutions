using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Search;
using WB.Core.GenericSubdomains.Portable;

namespace WB.UI.Designer.Controllers.Api.Designer
{
    [ResponseCache(NoStore = true)]
    [Authorize]
    [Route("api/search")]
    public class SearchController : Controller
    {
        private readonly IPublicFoldersStorage publicFoldersStorage;
        private readonly IQuestionnaireSearchStorage questionnaireSearchStorage;

        public SearchController(IPublicFoldersStorage publicFoldersStorage,
            IQuestionnaireSearchStorage questionnaireSearchStorage)
        {
            this.publicFoldersStorage = publicFoldersStorage;
            this.questionnaireSearchStorage = questionnaireSearchStorage;
        }
   
        [HttpGet]
        [Route("filters")]
        public Task<List<QuestionnaireListViewFolder>> GetFolders()
        {
            return publicFoldersStorage.GetAllFoldersAsync();
        }

        [HttpGet]
        [Route("")]
        public IActionResult Search([FromQuery] SearchQueryModel model)
        {
            var searchResult = questionnaireSearchStorage.Search(new SearchInput()
            {
                Query = model.Query,
                FolderId = model.FolderId,
                PageIndex = model.PageIndex,
                PageSize = model.PageSize,
            });

            var result = new SearchResultModel(
                searchResult.Items.Select(i =>
                    new QuestionnaireSearchResultEntity()
                    {
                        QuestionnaireId = i.QuestionnaireId.FormatGuid(),
                        QuestionnaireTitle = i.QuestionnaireTitle,
                        ItemId = i.EntityId.FormatGuid(),
                        ItemType = i.EntityType,
                        SectionId = i.SectionId.FormatGuid(),
                        Title = i.Title,
                        Folder = i.FolderId != null
                            ? new QuestionnaireListViewFolder(i.FolderId.Value, i.FolderName ?? "")
                            : null
                    }).ToList(), 
                searchResult.TotalCount
            );

            return Ok(result);
        }
    }

    public class SearchResultModel
    {
        public SearchResultModel(List<QuestionnaireSearchResultEntity> entities, int total)
        {
            Entities = entities;
            Total = total;
        }

        public List<QuestionnaireSearchResultEntity> Entities { get; set; }
        public int Total { get; set; }
    }

    public class QuestionnaireSearchResultEntity
    {
        public string? Title { get; set; }
        public string? QuestionnaireTitle { get; set; }
        public QuestionnaireListViewFolder? Folder { get; set; }
        public string? QuestionnaireId { get; set; }
        public string? SectionId { get; set; }
        public string? ItemId  { get; set; }
        public string? ItemType { get; set; }
    }

    public class SearchQueryModel
    {
        public string? Query { get; set; }
        public Guid? FolderId{ get; set; }
        public bool PrivateOnly { get; set; } = false;

        public int PageIndex { get; set; }
        public int PageSize { get; set; } = 20;
    }
}
