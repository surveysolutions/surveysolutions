using System;

namespace WB.Core.BoundedContexts.Headquarters.Team.Models
{
    public class InterviewersItem
    {
        public InterviewersItem(Guid id, string name, string email, DateTime creationDate, bool isLocked)
        {
            this.UserId = id;
            this.UserName = name;
            this.Email = email;
            this.CreationDate = creationDate.ToShortDateString();
            this.IsLocked = isLocked;
        }

        public bool IsLocked { get; private set; }

        public string CreationDate { get; private set; }

        public string Email { get; private set; }

        public Guid UserId { get; private set; }

        public string UserName { get; private set; }
    }
}