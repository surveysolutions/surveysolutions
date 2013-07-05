using System;
using Core.Supervisor.Views.Interviewer;
using Main.Core.Commands.User;
using WB.Core.GenericSubdomains.Logging;
using Web.Supervisor.Models;

namespace Web.Supervisor.Controllers
{
    using System.Web.Http;

    using Main.Core.View;

    using Ncqrs.Commanding.ServiceModel;

    using Questionnaire.Core.Web.Helpers;

    [Authorize(Roles = "Headquarter, Supervisor")]
    public class UsersApiController : BaseApiController
    {
        private readonly IViewFactory<InterviewersInputModel, InterviewersView> users;

        public UsersApiController(
            ICommandService commandService,
            IGlobalInfoProvider provider,
            ILogger logger,
            IViewFactory<InterviewersInputModel, InterviewersView> users)
            : base(commandService, provider, logger)
        {
            this.users = users;
        }

        public InterviewersView Interviewers(UsersListViewModel data)
        {
            var input = new InterviewersInputModel(this.GlobalInfo.GetCurrentUser().Id)
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

            return this.users.Load(input);
        }

        /// <summary>
        /// Lock user
        /// </summary>
        /// <param name="id">
        /// Use public key
        /// </param>
        /// <returns>
        /// Redirects to index view if everything is ok
        /// </returns>
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
