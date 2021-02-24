using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Runtime.Internal;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.DeleteQuestionnaireTemplate;
using WB.Core.BoundedContexts.Headquarters.Services.DynamicReporting;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Infrastructure.Native.Utils;
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

        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IDeleteQuestionnaireService deleteQuestionnaireService;
        private readonly IWebInterviewConfigProvider webInterviewConfigProvider;

        private readonly IExposedVariablesService exposedVariablesService;
        private readonly IPlainStorageAccessor<QuestionnaireCompositeItem> questionnaireItems;

        public QuestionnairesApiController(IAuthorizedUser authorizedUser, 
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IDeleteQuestionnaireService deleteQuestionnaireService,
            IWebInterviewConfigProvider webInterviewConfigProvider,
            IPlainStorageAccessor<QuestionnaireCompositeItem> questionnaireItems,
            IExposedVariablesService exposedVariablesService,
            IQuestionnaireStorage questionnaireStorage)
        {
            this.authorizedUser = authorizedUser;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.deleteQuestionnaireService = deleteQuestionnaireService;
            this.webInterviewConfigProvider = webInterviewConfigProvider;
            this.questionnaireItems = questionnaireItems;
            this.exposedVariablesService = exposedVariablesService;
            this.questionnaireStorage = questionnaireStorage;
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

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> ChangeVariableExposeStatus([FromBody] UpdateExposedVariablesRequestModel request)
        {
            if (!QuestionnaireIdentity.TryParse(request.QuestionnaireIdentity, out QuestionnaireIdentity questionnaireIdentity))
            {
                return this.BadRequest();
            }

            await exposedVariablesService.UpdateExposedVariables(questionnaireIdentity, request.Variables, this.authorizedUser.Id);
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

        [HttpGet]
        [Authorize(Roles = "Administrator, Headquarter")]
        public List<QuestionnaireExposableEntity> GetQuestionnaireExposedVariables([FromQuery] string id)
        {
            if (!QuestionnaireIdentity.TryParse(id, out QuestionnaireIdentity questionnaireIdentity))
            {
                return null;
            }

            return this.questionnaireItems.Query(_ => _
                    .Where(x => x.QuestionnaireIdentity == questionnaireIdentity.ToString() && x.UsedInReporting == true)
                    .OrderUsingSortExpression("Id Asc")
                    .Select(x => new QuestionnaireExposableEntity
                    {
                        Id = x.Id,
                        Title = x.QuestionText,
                        Variable = x.StataExportCaption,
                        Label = x.VariableLabel ?? "",
                        IsExposed = x.UsedInReporting ?? false,
                        EntityType = x.EntityType
                    }))
                .ToList();
        }

        [HttpGet]
        [Authorize(Roles = "Administrator, Headquarter")]
        public DataTableResponse<QuestionnaireExposableEntity> GetQuestionnaireVariables([Models.Api.DataTable.DataTablesRequest] DataTableRequest request,
            [FromQuery] string id)
        {
            if (!QuestionnaireIdentity.TryParse(id, out QuestionnaireIdentity questionnaireIdentity))
            {
                return null;
            }

            var document = this.questionnaireStorage.GetQuestionnaireDocument(questionnaireIdentity);
            var questionnaire = this.questionnaireStorage.GetQuestionnaire(questionnaireIdentity, null);

            var variablesNotInRostersIds = document.Children
                .Where(y=>(y as Group)?.IsRoster != true)
                .TreeToEnumerable(x => x.Children.Where(y=>(y as Group)?.IsRoster != true))
                .Where(x =>x is IVariable)
                .Select(x => x.PublicKey);

            var questionsNotInRosters = document.Children
                .Where(y => (y as Group)?.IsRoster != true)
                .TreeToEnumerable(x =>x.Children.Where(y=>(y as Group)?.IsRoster != true))
                .OfType<IQuestion>()
                .Where(x=>x.QuestionType == QuestionType.DateTime ||
                          x.QuestionType == QuestionType.Numeric ||
                          x.QuestionType == QuestionType.Text ||
                          (x.QuestionType == QuestionType.SingleOption 
                           && !x.LinkedToQuestionId.HasValue 
                           && !x.LinkedToRosterId.HasValue 
                           && x.CascadeFromQuestionId == null
                           && x.IsFilteredCombobox != true))
                .Select(x => x.PublicKey);

            var permittedEntities = questionsNotInRosters.Union(variablesNotInRostersIds).Except(questionnaire.GetPrefilledEntities()).ToList();

            var variables = this.questionnaireItems.Query(q =>
            {
                q = q.Where(i => i.QuestionnaireIdentity == questionnaireIdentity.ToString());
                q = q.Where(i => permittedEntities.Contains(i.EntityId));

                if (request.Search?.Value != null)
                {
                    var search = request.Search.Value.ToLower();
                    q = q.Where(i => i.QuestionText.Contains(search)
                                     || i.StataExportCaption.ToLower().Contains(search)
                                     || i.VariableLabel.Contains(search));
                }

                q = q.OrderUsingSortExpression("Id Asc");

                IQueryable<QuestionnaireCompositeItem> pagedResults = q;
                
                if (request.PageSize > 0)
                {
                    pagedResults = q.Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize);
                }

                return new QuestionnaireExposableEntities()
                {
                    TotalCount = q.Count(),
                    Items = pagedResults.Select(x=> new QuestionnaireEntityItem()
                    {
                        Title = x.QuestionText,
                        Id = x.Id,
                        VariableName = x.StataExportCaption,
                        IsExposed = x.UsedInReporting,
                        Label = x.VariableLabel,
                        EntityType = x.EntityType
                        
                    }).ToList()
                };
            });

            
            return new DataTableResponse<QuestionnaireExposableEntity>
            {
                Draw = request.Draw + 1,
                RecordsTotal = variables.TotalCount,
                RecordsFiltered = variables.TotalCount,
                Data = variables.Items.Select(x => new QuestionnaireExposableEntity
                {
                    Id = x.Id,
                    Title = x.Title,
                    Variable = x.VariableName,
                    IsExposed = x.IsExposed ?? false,
                    EntityType = x.EntityType,
                    Label = x.Label ?? ""
                })
            };
        }
    }

    public class QuestionnaireExposableEntities
    {
        public QuestionnaireExposableEntities()
        {
            Items = new List<QuestionnaireEntityItem>();
        }
        public IEnumerable<QuestionnaireEntityItem> Items { get; set; }
        public int TotalCount { get; set; }
    }

    public class QuestionnaireExposableEntity
    {
        public int Id { get; set; }
        public string Title { set; get; }
        public string Label { set; get; }
        public bool IsExposed { set; get; }
        public string Variable { get; set; }
        public EntityType EntityType { get; set; }
    }

    public class QuestionnaireEntityItem
    {
        public int Id { get; set; }
        public string Title { set; get; }
        public string Label { set; get; }
        public bool? IsExposed { set; get; }
        public string VariableName { get; set; }
        public EntityType EntityType { get; set; }
    }
}
