using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.DeleteQuestionnaireTemplate;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models.Api;
using WB.UI.Headquarters.Models.ComponentModels;
using WB.UI.Headquarters.Models.Template;

namespace WB.UI.Headquarters.Controllers.Api
{
    [ApiValidationAntiForgeryToken]
    [Route("api/[controller]/[action]/{id?}")]
    [ResponseCache(NoStore = true)]
    public class QuestionnairesApiController : ControllerBase
    {
        private readonly IAuthorizedUser authorizedUser;
        private const int DEFAULTPAGESIZE = 12;
        private const string DEFAULTEMPTYQUERY = "";

        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IDeleteQuestionnaireService deleteQuestionnaireService;
        private readonly IWebInterviewConfigProvider webInterviewConfigProvider;

        public QuestionnairesApiController(IAuthorizedUser authorizedUser, 
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IDeleteQuestionnaireService deleteQuestionnaireService,
            IWebInterviewConfigProvider webInterviewConfigProvider)
        {
            this.authorizedUser = authorizedUser;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.deleteQuestionnaireService = deleteQuestionnaireService;
            this.webInterviewConfigProvider = webInterviewConfigProvider;
        }

        [HttpGet]
        [Authorize(Roles = "Administrator, Headquarter, Interviewer")]
        public DataTableResponse<QuestionnaireListItemModel> Questionnaires([Models.Api.DataTable.DataTablesRequest] DataTableRequest request)
        {
            var input = new QuestionnaireBrowseInputModel
            {
                Page = request.PageIndex,
                PageSize = request.PageSize,
                Orders = request.GetSortOrderRequestItems(),
                SearchFor = request.Search?.Value,
                IsAdminMode = true
            };

            var items = this.questionnaireBrowseViewFactory.Load(input);

            return new DataTableResponse<QuestionnaireListItemModel>
            {
                Draw = request.Draw + 1,
                RecordsTotal = items.TotalCount,
                RecordsFiltered = items.TotalCount,
                Data = items.Items.ToList().Select(x => new QuestionnaireListItemModel
                {
                    Id = x.Id,
                    QuestionnaireId = x.QuestionnaireId,
                    Version = x.Version,
                    Title = x.Title,
                    AllowCensusMode = x.AllowCensusMode,
                    CreationDate = x.CreationDate,
                    LastEntryDate = x.LastEntryDate,
                    ImportDate = x.ImportDate,
                    IsDisabled = x.Disabled,
                    IsAudioRecordingEnabled = x.IsAudioRecordingEnabled,
                    WebModeEnabled = this.webInterviewConfigProvider.Get(new QuestionnaireIdentity(x.QuestionnaireId, x.Version))?.Started ?? false //not efficient
                })
            };
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Headquarter")]
        public QuestionnaireBrowseView AllQuestionnaires(AllQuestionnairesListViewModel data)
        {
            var input = new QuestionnaireBrowseInputModel
            {
                Page = data.PageIndex,
                PageSize = data.PageSize,
                Orders = data.SortOrder ?? new List<OrderRequestItem>(),
                SearchFor = data.SeachFor
            };

            return this.questionnaireBrowseViewFactory.Load(input);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteQuestionnaire([FromBody]DeleteQuestionnaireRequestModel request)
        {
            await deleteQuestionnaireService.DisableQuestionnaire(request.QuestionnaireId, request.Version, this.authorizedUser.Id);
            
            return Ok();
        }

        [HttpGet]
        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        public ComboboxModel QuestionnairesWithVersions(Guid? id = null, string query = DEFAULTEMPTYQUERY, int pageSize = DEFAULTPAGESIZE)
        {
            if (id != null) 
            {
                var questionnaires = this.questionnaireBrowseViewFactory.Load(new QuestionnaireBrowseInputModel {
                    PageSize = pageSize,
                    QuestionnaireId = id,
                    SearchFor = query,
                    IsAdminMode = true,
                    Order = nameof(QuestionnaireBrowseItem.Version) + " DESC"
                });
                return new ComboboxModel(questionnaires.Items.Select(x => new ComboboxOptionModel(x.Version.ToString(),
                    $"ver. {x.Version}")).ToArray(), questionnaires.TotalCount);
            }

            var questionnaireNames = this.questionnaireBrowseViewFactory.UniqueQuestionnaireIds(query, pageSize);

            return new ComboboxModel(questionnaireNames.Items.Select(x => new ComboboxOptionModel(x.Id.FormatGuid(), x.Title)).ToArray(), questionnaireNames.TotalCount);
        }

        [HttpGet]
        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        public ComboboxModel QuestionnairesCombobox(string query = DEFAULTEMPTYQUERY, int pageSize = DEFAULTPAGESIZE, bool censusOnly = false)
        {
            var questionnaires = this.questionnaireBrowseViewFactory.Load(new QuestionnaireBrowseInputModel
            {
                PageSize = pageSize,
                SearchFor = query,
                IsAdminMode = true,
                OnlyCensus = censusOnly
            });

            return new ComboboxModel(questionnaires.Items.Select(x => new ComboboxOptionModel(x.Id, $"(ver. {x.Version}) {x.Title}")).ToArray(), questionnaires.TotalCount);
        }

        [HttpGet]
        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        public IActionResult QuestionnairesComboboxById(string questionnaireIdentity, bool censusOnly = false)
        {
            var identity = QuestionnaireIdentity.Parse(questionnaireIdentity);

            var questionnaires = this.questionnaireBrowseViewFactory.Load(new QuestionnaireBrowseInputModel
            {
                QuestionnaireId = identity.QuestionnaireId,
                Version = identity.Version,
                IsAdminMode = true,
                OnlyCensus = censusOnly
            });

            var questionnaireItems = questionnaires.Items.ToList();
            if (questionnaireItems.Count > 0)
            {
                var firstHit = questionnaireItems[0];
                return Ok(
                    new
                    {
                        Id = firstHit.Id,
                        Title = firstHit.Title,
                        Version = firstHit.Version
                    });
            }

            return NotFound();
        }
    }
}
