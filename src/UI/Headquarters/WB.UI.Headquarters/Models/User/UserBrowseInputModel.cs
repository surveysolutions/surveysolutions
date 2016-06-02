using System;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models.User
{
    public class UserBrowseInputModel
    {
        private int _page = 1;

        private int _pageSize = 20;

        public UserBrowseInputModel()
        {
        }

        public UserBrowseInputModel(UserRoles role)
        {
            this.Role = role;
        }

        public Guid LocationId { get; set; }

        public int Page
        {
            get
            {
                return this._page;
            }

            set
            {
                this._page = value;
            }
        }

        public int PageSize
        {
            get
            {
                return this._pageSize;
            }

            set
            {
                this._pageSize = value;
            }
        }

        public UserRoles? Role { get; private set; }
    }
}