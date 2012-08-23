using System;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.User
{
    public class InterviewersItem
    {
        private string _id;

        public InterviewersItem(string id, string name, string email, DateTime creationDate, bool isLocked, int total, int completed, int inProcess)
        {
            Id = id;
            Login = name;
            Email = email;
            CreationDate = creationDate;
            IsLocked = isLocked;
            Total = total;
            Completed = completed;
            InProcess = inProcess;
        }

        public string Id
        {
            get { return IdUtil.ParseId(_id); }
            private set { _id = value; }
        }

        public string Login { get; private set; }

        public DateTime CreationDate { get; private set; }

        public string Email { get; private set; }

        public bool IsLocked { get; private set; }

        public int Total { get; private set; }

        public int Completed { get; private set; }

        public int InProcess { get; private set; }
    }
}