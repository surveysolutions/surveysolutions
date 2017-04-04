﻿using System;
using System.Linq;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.BrokenInterviewPackages;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Models.Api;
using WB.UI.Headquarters.Models.Interview;

namespace WB.UI.Headquarters.API
{
    [Authorize]
    [CamelCase]
    public class TroubleshootingApiController : BaseApiController
    {
        private readonly IAllInterviewsFactory allInterviewsViewFactory;
        private readonly IBrokenInterviewPackagesViewFactory brokenInterviewPackagesViewFactory;
        private readonly ITroubleshootingService troubleshootingService;

        public TroubleshootingApiController(
            IAllInterviewsFactory allInterviewsViewFactory, 
            ICommandService commandService, 
            ILogger logger, 
            IBrokenInterviewPackagesViewFactory brokenInterviewPackagesViewFactory, 
            ITroubleshootingService troubleshootingService) 
            : base(commandService, logger)
        {
            this.allInterviewsViewFactory = allInterviewsViewFactory;
            this.brokenInterviewPackagesViewFactory = brokenInterviewPackagesViewFactory;
            this.troubleshootingService = troubleshootingService;
        }

        [HttpPost]
        [CamelCase]
        public DataTableResponse<InterviewListItem> MissingData([FromBody] TroubleshootingMissingInterviewsDataTableRequest request)
        {
            Guid parsedInterviewId;
            Guid? interviewId = Guid.TryParse(request.InterviewId, out parsedInterviewId) ? parsedInterviewId: (Guid?)null;
            string interviewKey = interviewId.HasValue ? null : request.InterviewId;

            var input = new InterviewsWithoutPrefilledInputModel
            {
                Page = request.PageIndex,
                PageSize = request.PageSize,
                Orders = request.GetSortOrderRequestItems(),
                InterviewKey = interviewKey,
                InterviewId = interviewId,
                SearchBy = request.Search.Value
            };

            var items = this.allInterviewsViewFactory.LoadInterviewsWithoutPrefilled(input);

            var interview = items.Items.FirstOrDefault();

            string message = troubleshootingService.GetMissingDataReason(interviewId, interviewKey);
            
            return new TroubleshootingMissingInterviewsDataTableResponse
            {
                Draw = request.Draw + 1,
                RecordsTotal = items.TotalCount,
                RecordsFiltered = items.TotalCount,
                Data = items.Items.ToList(),
                Message = message,
                InterviewKey = interview?.Key
            };
        }

        [HttpPost]
        [CamelCase]
        public DataTableResponse<InterviewListItem> CensusInterviews([FromBody] TroubleshootingCensusInterviewsDataTableRequest request)
        {
            QuestionnaireIdentity questionnaireIdentity = null;
            QuestionnaireIdentity.TryParse(request.QuestionnaireId, out questionnaireIdentity);

            var changedFrom = (request.ChangedFrom ?? DateTime.Now.AddMonths(-1));
            var changedTo = (request.ChangedTo ?? DateTime.Now).AddDays(1); 
            var input = new InterviewsWithoutPrefilledInputModel
            {
                Page = request.PageIndex,
                PageSize = request.PageSize,
                Orders = request.GetSortOrderRequestItems(),
                QuestionnaireId = questionnaireIdentity,
                CensusOnly = true,
                ChangedFrom = changedFrom,
                ChangedTo = changedTo,
                InterviewerId = request.InterviewerId,
                SearchBy = request.Search.Value
                
            };

            var items = this.allInterviewsViewFactory.LoadInterviewsWithoutPrefilled(input);

            BrokenInterviewPackagesView brokenItems = this.brokenInterviewPackagesViewFactory.GetFilteredItems(new BrokenInterviewPackageFilter
            {
                QuestionnaireIdentity = request.QuestionnaireId,
                FromProcessingDateTime = changedFrom,
                ToProcessingDateTime = changedTo,
                ResponsibleId = request.InterviewerId
            });

            return new TroubleshootingCensusInterviewsDataTableResponse
            {
                Draw = request.Draw + 1,
                RecordsTotal = items.TotalCount,
                RecordsFiltered = items.TotalCount,
                Data = items.Items.ToList(),
                BrokenPackagesCount = brokenItems.TotalCount,
                CensusInterviewsCount = items.TotalCount
            };
        }
    }
}
