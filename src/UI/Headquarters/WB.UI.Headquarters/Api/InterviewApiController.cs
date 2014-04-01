using System.Web.Http;
using Main.Core.View;
using WB.Core.BoundedContexts.Headquarters.Authentication;
using WB.Core.BoundedContexts.Headquarters.Interview.Views;
using WB.UI.Headquarters.Api.Models;

namespace WB.UI.Headquarters.Api
{
    [Authorize(Roles = ApplicationRoles.Headquarter)]
    public class InterviewApiController : ApiController
    {
        private readonly IViewFactory<AllInterviewsInputModel, AllInterviewsView> allInterviewsViewFactory;

        public InterviewApiController(IViewFactory<AllInterviewsInputModel, AllInterviewsView> allInterviewsViewFactory)
        {
            this.allInterviewsViewFactory = allInterviewsViewFactory;
        }

        [HttpPost]
        public AllInterviewsView AllInterviews(DocumentListViewModel data)
        {
            var input = new AllInterviewsInputModel
                {
                    Orders = data.SortOrder
                };
            if (data.Pager != null)
            {
                input.Page = data.Pager.Page;
                input.PageSize = data.Pager.PageSize;
            }

            if (data.Request != null)
            {
                input.QuestionnaireId = data.Request.TemplateId;
                input.QuestionnaireVersion = data.Request.TemplateVersion;
                input.TeamLeadId = data.Request.ResponsibleId;
                input.Status = data.Request.Status;
            }

            return this.allInterviewsViewFactory.Load(input);
        }
    }
}