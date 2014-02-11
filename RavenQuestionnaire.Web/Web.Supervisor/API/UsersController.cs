using System;
using System.Collections.Generic;
using System.Web.Http;
using Core.Supervisor.Views.Interviewer;
using Core.Supervisor.Views.User;
using Main.Core.Entities;
using Main.Core.Entities.SubEntities;
using Main.Core.View;
using WB.Core.GenericSubdomains.Logging;
using Web.Supervisor.Models;

namespace Web.Supervisor.API
{
    //[RoutePrefix("api/v1/supervisors")]
    [Authorize/*(Roles = "Headquarter")*/]
    public class UsersController : BaseApiServiceController
    {
        private readonly IViewFactory<InterviewersInputModel, InterviewersView> interviewersFactory;
        private readonly IViewFactory<UserListViewInputModel, UserListView> supervisorsFactory;
        private readonly IViewFactory<UserViewInputModel, UserView> userViewFactory;
        

        public UsersController(ILogger logger, 
            IViewFactory<InterviewersInputModel, InterviewersView> interviewersFactory,
            IViewFactory<UserListViewInputModel, UserListView> supervisorsFactory,
            IViewFactory<UserViewInputModel, UserView> userViewFactory)
            :base(logger)
        {
            this.interviewersFactory = interviewersFactory;
            this.supervisorsFactory = supervisorsFactory;
            this.userViewFactory = userViewFactory;
        }
        
        public UserListView Supervisors(int limit, int offset)
        {
            var data = new UsersListViewModel
            {
                SortOrder = new List<OrderRequestItem>() { }
            };

            var input = new UserListViewInputModel
            {
                Page = offset,
                PageSize = limit,
                Role = UserRoles.Supervisor,
                Orders = data.SortOrder
            };

            return this.supervisorsFactory.Load(input);
        }

        
        public UserView Supervisors(Guid id)
        {
            return this.userViewFactory.Load(new UserViewInputModel(id));
        }


        public InterviewersView Intervievers(Guid supervisorId, int limit, int offset)
        {
            var input = new InterviewersInputModel(supervisorId)
            {
                Page = offset,
                PageSize = limit,
            };

            return this.interviewersFactory.Load(input);
        }

        public UserView Details(Guid id)
        {
            return this.userViewFactory.Load(new UserViewInputModel(id));
        }
    }
}