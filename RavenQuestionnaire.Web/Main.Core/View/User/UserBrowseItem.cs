using System;
using Main.Core.Entities.SubEntities;

namespace Main.Core.View.User
{
    /// <summary>
    /// The user browse item.
    /// </summary>
    public class UserBrowseItem
    {
        public UserBrowseItem(Guid id, string name, string email, DateTime creationDate, bool isLocked, UserLight supervisor)
        {
            this.Id = id;
            this.Email = email;
            this.UserName = name;
            this.IsLocked = isLocked;
            this.CreationDate = creationDate;
            if (supervisor != null) 
                this.SupervisorName = supervisor.Name;
        }

        public DateTime CreationDate { get; private set; }

        public string Email { get; private set; }

        public Guid Id { get; private set; }

        public bool IsLocked { get; private set; }

        public string SupervisorName { get; set; }

        public string UserName { get; private set; }
    }
}