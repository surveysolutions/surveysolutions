using System;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.SurveyManagement.Views.Interviewer;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
    public class UsersApiController : BaseApiController
    {
        private readonly IViewFactory<InterviewersInputModel, InterviewersView> interviewersFactory;
        private readonly IUserListViewFactory supervisorsFactory;

        public UsersApiController(
            ICommandService commandService,
            IGlobalInfoProvider provider,
            ILogger logger,
            IViewFactory<InterviewersInputModel, InterviewersView> interviewersFactory,
            IUserListViewFactory supervisorsFactory)
            : base(commandService, provider, logger)
        {
            this.interviewersFactory = interviewersFactory;
            this.supervisorsFactory = supervisorsFactory;
        }

        public InterviewersView Interviewers(UsersListViewModel data)
        {
            // Headquarter and Admin can view interviewers by any supervisor
            // Supervisor can view only their interviewers
            Guid? viewerId = this.GlobalInfo.IsHeadquarter || this.GlobalInfo.IsAdministrator
                                 ? data.Request.SupervisorId
                                 : this.GlobalInfo.GetCurrentUser().Id;
            if (viewerId != null)
            {
                var input = new InterviewersInputModel(viewerId.Value)
                    {
                        Orders = data.SortOrder,
                        SearchBy = data.SearchBy
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

        [Authorize(Roles = "Administrator, Headquarter")]
        public UserListView Supervisors(UsersListViewModel data)
        {
            var input = new UserListViewInputModel
                {
                    Role = UserRoles.Supervisor,
                    Orders = data.SortOrder,
                    SearchBy = data.SearchBy
                };

            if (data.Pager != null)
            {
                input.Page = data.Pager.Page;
                input.PageSize = data.Pager.PageSize;
            }

            UserListView result = this.supervisorsFactory.Load(input);
            return result;
        }

        [Authorize(Roles = "Administrator")]
        public UserListView Hqs(UsersListViewModel data)
        {
            var input = new UserListViewInputModel
            {
                Role = UserRoles.Headquarter,
                Orders = data.SortOrder,
                SearchBy = data.SearchBy
            };

            if (data.Pager != null)
            {
                input.Page = data.Pager.Page;
                input.PageSize = data.Pager.PageSize;
            }

            UserListView result = this.supervisorsFactory.Load(input);
            return result;
        }
    }
}