using System;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using Main.Core.View;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.SurveyManagement.Views.Interviewer;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    [Authorize(Roles = "Headquarter, Supervisor")]
    public class UsersApiController : BaseApiController
    {
        private readonly IViewFactory<InterviewersInputModel, InterviewersView> interviewersFactory;
        private readonly IViewFactory<UserListViewInputModel, UserListView> supervisorsFactory;

        public UsersApiController(
            ICommandService commandService,
            IGlobalInfoProvider provider,
            ILogger logger,
            IViewFactory<InterviewersInputModel, InterviewersView> interviewersFactory,
            IViewFactory<UserListViewInputModel, UserListView> supervisorsFactory)
            : base(commandService, provider, logger)
        {
            this.interviewersFactory = interviewersFactory;
            this.supervisorsFactory = supervisorsFactory;
        }

        public InterviewersView Interviewers(UsersListViewModel data)
        {
            // Headquarter can view interviewers by any supervisor
            // Supervisor can view only their interviewers
            Guid? viewerId = this.GlobalInfo.IsHeadquarter
                                 ? data.Request.SupervisorId
                                 : this.GlobalInfo.GetCurrentUser().Id;
            if (viewerId != null)
            {
                var input = new InterviewersInputModel(viewerId.Value)
                    {
                        Orders = data.SortOrder
                    };
                if (data.Pager != null)
                {
                    input.Page = data.Pager.Page;
                    input.PageSize = data.Pager.PageSize;
                }

                return this.interviewersFactory.Load(input);
            }

            return null;
        }

        public UserListView Supervisors(UsersListViewModel data)
        {
            var input = new UserListViewInputModel
                {
                    Role = UserRoles.Supervisor,
                    Orders = data.SortOrder
                };

            if (data.Pager != null)
            {
                input.Page = data.Pager.Page;
                input.PageSize = data.Pager.PageSize;
            }

            return this.supervisorsFactory.Load(input);
        }
    }
}