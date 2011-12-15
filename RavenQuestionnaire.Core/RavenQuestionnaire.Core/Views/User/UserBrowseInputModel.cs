using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.User
{
    public class UserBrowseInputModel
    {
        public int Page
        {
            get { return _page; }
            set { _page = value; }
        }

        private int _page = 1;
        public int PageSize
        {
            get { return _pageSize; }
            set { _pageSize = value; }
        }
        private int _pageSize = 5;

        public UserRoles? Role { get; private set; }
        public string LocationId { get; set; }

        public UserBrowseInputModel()
        {
        }
        public UserBrowseInputModel(UserRoles role)
        {
            Role = role;
        }

        public Func<UserDocument, bool> Expression
        {
            get
            {
                if (!string.IsNullOrEmpty(LocationId))
                {
                    string locatianOriginalId = IdUtil.CreateLocationId(LocationId);
                    return e => !e.IsDeleted && e.Location.Id == locatianOriginalId;
                }

                if (!Role.HasValue)
                    return e => !e.IsDeleted;
                return e => !e.IsDeleted && e.Roles.Contains(Role.Value);
            }
        }
    }
}
