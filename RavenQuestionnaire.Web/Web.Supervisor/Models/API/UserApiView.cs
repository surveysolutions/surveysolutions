using System;
using System.Collections.Generic;
using System.Linq;
using Core.Supervisor.Views.Interviewer;
using Core.Supervisor.Views.User;

namespace Web.Supervisor.Models.API
{
    public class UserApiView : BaseApiView
    {
        public UserApiView(int page, int pageSize, int totalCount, IEnumerable<UserApiItem> users, string order)
        {
            this.Offset = page;
            this.TotalCount = totalCount;
            this.Limit = pageSize;
            this.Users = users;
            this.Order = order;
        }

        public UserApiView(UserListView userListView)
        {
            if (userListView == null)
                return;

            this.TotalCount = userListView.TotalCount;
            this.Users = userListView.Items.Select(
                    item => new UserApiItem(item.UserId, item.UserName, item.Email, DateTime.Parse(item.CreationDate), item.IsLocked));

            this.Offset = userListView.Page;
            this.Limit = userListView.PageSize;
            //this.Order = questionnaireBrowseView.Order;
        }

        public UserApiView(InterviewersView userListView)
        {
            if (userListView == null)
                return;

            this.TotalCount = userListView.TotalCount;
            this.Users = userListView.Items.Select(
                    item => new UserApiItem(item.UserId, item.UserName, item.Email, DateTime.Parse(item.CreationDate), item.IsLocked));

            //this.Offset = questionnaireBrowseView.ItemsSummary.Page;
            //this.Limit = questionnaireBrowseView.PageSize;
            //this.Order = questionnaireBrowseView.Order;
        }

        public IEnumerable<UserApiItem> Users { get; private set; }
    }
}