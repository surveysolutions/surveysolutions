using System.Linq;
using System.Web.Http;
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

        public TroubleshootingApiController(
            IAllInterviewsFactory allInterviewsViewFactory, 
            ICommandService commandService, 
            ILogger logger) 
            : base(commandService, logger)
        {
            this.allInterviewsViewFactory = allInterviewsViewFactory;
        }

        [HttpPost]
        [CamelCase]
        public DataTableResponse<InterviewSummary> CensusInterviews([FromBody] TroubleshootingCensusInterviewsDataTableRequest request)
        {
            QuestionnaireIdentity questionnaireIdentity = null;
            QuestionnaireIdentity.TryParse(request.QuestionnaireId, out questionnaireIdentity);

            var input = new InterviewsWithoutPrefilledInputModel
            {
                Page = request.PageIndex,
                PageSize = request.PageSize,
                //Orders = request.GetSortOrderRequestItems(),
                QuestionnaireId = questionnaireIdentity,
                CensusOnly = true,
                ChangedFrom = request.ChangedFrom,
                ChangedTo = request.ChangedTo,
                InterviewerId = request.InterviewerId,
            };

            var items = this.allInterviewsViewFactory.LoadInterviewsWithoutPrefilled(input);

            return new DataTableResponse<InterviewSummary>
            {
                Draw = request.Draw + 1,
                RecordsTotal = items.TotalCount,
                RecordsFiltered = items.TotalCount,
                Data = items.Items.ToList()
            };
        }
    }
}
