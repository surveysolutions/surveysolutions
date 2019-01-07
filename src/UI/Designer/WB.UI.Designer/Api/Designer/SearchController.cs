﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Search;
using WB.Core.GenericSubdomains.Portable;
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
        public Task<SearchResultModel> Search([FromUri] SearchQueryModel model)
        {
            var searchResult = questionnaireSearchStorage.Search(new SearchInput()
            {
                Query = model.Query,
                FolderId = model.FolderId,
                PageIndex = model.PageIndex,
                PageSize = model.PageSize,
            });

            var result = new SearchResultModel()
            {
                Total = searchResult.TotalCount,
                Entities = searchResult.Items.Select(i =>
                    new QuestionnaireSearchResultEntity()
                    {
                        QuestionnaireId = i.QuestionnaireId.FormatGuid(),
                        QuestionnaireTitle = i.QuestionnaireTitle,
                        ItemId = i.EntityId.FormatGuid(),
                        ItemType = i.EntityType,
                        SectionId = i.SectionId.FormatGuid(),
                        Title = i.Title,
                        IsRoster = false,
                        Folder = !i.FolderId.HasValue
                            ? null
                            : new QuestionnaireListViewFolder()
                            {
                                Title = i.FolderName,
                                PublicId = i.FolderId.Value
                            }
                    }).ToList()
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
        public bool IsRoster { get; set; }
    }

    public class SearchQueryModel
    {
        public string Query { get; set; }
        public Guid? FolderId{ get; set; }
        public bool PrivateOnly { get; set; } = false;

        public int PageIndex { get; set; }
        public int PageSize { get; set; } = 20;
    }
}
