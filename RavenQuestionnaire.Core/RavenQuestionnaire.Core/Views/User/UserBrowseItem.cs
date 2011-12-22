using System;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.User
{
    public class UserBrowseItem
    {
        public UserBrowseItem(string id, string name, string email, DateTime creationDate, bool isLocked, UserLight supervisor, string location)
        {
            this.Id = id;
            this.UserName = name;
            this.Email = email;
            this.CreationDate = creationDate;
            this.IsLocked = isLocked;
            if (supervisor != null)
                this.SupervisorName = supervisor.Name;
            this.LocationName = location;
        }

        public string Id
        {
            get { return IdUtil.ParseId(_id); }
            private set { _id = value; }
        }

        private string _id;

        public string UserName { get;private set; }

        public string Email { get;private set; }

        public DateTime CreationDate { get; private set; }

        public bool IsLocked { get; private set; }

        public string SupervisorName { get; set; }

        public string LocationName { get; set; }

    }
}
