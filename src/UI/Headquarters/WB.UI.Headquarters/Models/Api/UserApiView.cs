using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.SurveyManagement.Views.Interviewer;
using WB.Core.SharedKernels.SurveyManagement.Views.User;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models.Api
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
                    item => new UserApiItem(
                        item.UserId, 
                        item.UserName, 
                        item.Email, 
                        DateTime.Parse(item.CreationDate), 
                        item.IsLockedByHQ || item.IsLockedBySupervisor,
                        item.DeviceId));

            this.Offset = userListView.Page;
            this.Limit = userListView.PageSize;
        }

        public UserApiView(InterviewersView userListView)
        {
            if (userListView == null)
                return;

            this.TotalCount = userListView.TotalCount;
            this.Users = userListView.Items.Select(
                    item => new UserApiItem(
                        item.UserId, 
                        item.UserName, 
                        item.Email, 
                        DateTime.Parse(item.CreationDate), 
                        item.IsLockedByHQ || item.IsLockedBySupervisor, 
                        item.DeviceId));
        }

        public IEnumerable<UserApiItem> Users { get; private set; }
    }
}