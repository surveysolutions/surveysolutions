using System;
using Core.Supervisor.Views.Interviewer;
using Core.Supervisor.Views.User;
using Main.Core.Commands.User;
using Main.Core.Entities.SubEntities;
using Web.Supervisor.Models;

namespace Web.Supervisor.Controllers
{
    using System.Web.Http;

    using Main.Core.View;

    using Ncqrs.Commanding.ServiceModel;

    using Questionnaire.Core.Web.Helpers;

    using WB.Core.SharedKernel.Logger;

    [Authorize(Roles = "Headquarter, Supervisor")]
    public class UsersApiController : BaseApiController
    {
        private readonly IViewFactory<InterviewersInputModel, InterviewersView> interviewersFactory;
        private readonly IViewFactory<UserListViewInputModel, UserListView> supervisorsFactory;

        public UsersApiController(
            ICommandService commandService,
            IGlobalInfoProvider provider,
            ILog logger,
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
                    Orders
                        =
                        data
                            .SortOrder
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
                Orders
                    =
                    data
                        .SortOrder
            };

            if (data.Pager != null)
            {
                input.Page = data.Pager.Page;
                input.PageSize = data.Pager.PageSize;
            }

            return this.supervisorsFactory.Load(input);
        }

        /// <summary>
        /// Lock user
        /// </summary>
        public LockResult Lock(LockRequest request)
        {
            if (request.IsLocked)
            {
                CommandService.Execute(new UnlockUserCommand(request.UserId));
                
            }
            else
            {
                CommandService.Execute(new LockUserCommand(request.UserId));    
            }

            return new LockResult();
        }

        public class LockResult
        {
            public bool IsSuccess = true;
        }

        public class LockRequest
        {
            public Guid UserId { get; set; }
            public bool IsLocked { get; set; }
        }
    }
}
