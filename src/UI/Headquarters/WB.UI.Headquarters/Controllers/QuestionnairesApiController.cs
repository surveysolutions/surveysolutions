using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Raven.Client;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire.BrowseItem;
using WB.Core.SharedKernels.SurveyManagement.Implementation.ReadSide.Indexes;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Headquarter")]
    [ApiValidationAntiForgeryToken]
    public class QuestionnairesApiController : BaseApiController
    {
        private readonly IViewFactory<QuestionnaireBrowseInputModel, QuestionnaireBrowseView> questionnaireBrowseViewFactory;
        private readonly IReadSideRepositoryIndexAccessor indexAccessor;

        public QuestionnairesApiController(
            ICommandService commandService, IGlobalInfoProvider globalInfo, ILogger logger,
            IViewFactory<QuestionnaireBrowseInputModel, QuestionnaireBrowseView> questionnaireBrowseViewFactory,
            IReadSideRepositoryIndexAccessor indexAccessor)
            : base(commandService, globalInfo, logger)
        {
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.indexAccessor = indexAccessor;
        }

        [HttpPost]
        public QuestionnaireBrowseView AllQuestionnaires(AllQuestionnairesListViewModel data)
        {
            var input = new QuestionnaireBrowseInputModel()
            {
                Orders = data.SortOrder  == null ? new List<OrderRequestItem>() : data.SortOrder.ToList()
            };
            if (data.Pager != null)
            {
                input.Page = data.Pager.Page;
                input.PageSize = data.Pager.PageSize;
            }

            if (data.Request != null)
            {
                input.Filter = data.Request.Filter;
            }

            return this.questionnaireBrowseViewFactory.Load(input);
        }

        [HttpPost]
        public JsonCommandResponse DeleteQuestionnaire(DeleteQuestionnaireRequestModel request)
        {
            var response = new JsonCommandResponse() { IsSuccess = true };

            this.CommandService.Execute(new PrepareQuestionnaireForDelete(request.QuestionnaireId, request.Version,
                     this.GlobalInfo.GetCurrentUser().Id));

            string indexName = typeof(InterviewsSearchIndex).Name;
            var interviewByQuestionnaire = indexAccessor.Query<SeachIndexContent>(indexName)
                                     .Where(interview => !interview.IsDeleted && 
                                                          interview.QuestionnaireId == request.QuestionnaireId && 
                                                          interview.QuestionnaireVersion == request.Version)
                                    .ProjectFromIndexFieldsInto<InterviewSummary>();
            

            var hasInterviewDeletionExceptions = false;
            foreach (var interviewSummary in interviewByQuestionnaire)
            {
                try
                {
                    this.CommandService.Execute(new HardDeleteInterview(interviewSummary.InterviewId,
                        this.GlobalInfo.GetCurrentUser().Id));
                }
                catch(Exception e)
                {
                    this.Logger.Error(string.Format("Error on command of type ({0}) handling ", typeof(HardDeleteInterview)), e);
                    hasInterviewDeletionExceptions = true;
                }

            }

            if (hasInterviewDeletionExceptions)
            {
                response.DomainException = string.Format("Failed to delete one or more interviews which were created from questionnaire {0} version {1}.",
                        request.QuestionnaireId.FormatGuid(), request.Version);
            }
            else
            {
                try
                {
                    this.CommandService.Execute(new DeleteQuestionnaire(request.QuestionnaireId, request.Version,
                        this.GlobalInfo.GetCurrentUser().Id));
                }
                catch (Exception e)
                {
                    var domainEx = e.GetSelfOrInnerAs<QuestionnaireException>();
                    if (domainEx == null)
                    {
                        this.Logger.Error(string.Format("Error on command of type ({0}) handling ", typeof(DeleteQuestionnaire)), e);
                        throw;
                    }

                    response.DomainException = domainEx.Message;
                }    
            }

            return response;
        }
    }
}