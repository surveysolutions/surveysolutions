using System.Collections.Generic;

namespace RavenQuestionnaire.Core.Views.User
{
    public class InterviewerView
    {
        public string UserName { get; private set; }

        public string UserId { get; private set; }

        public List<InterviewerGroupView> Items { get; private set; }

        public InterviewerView(string userName, string userId, List<InterviewerGroupView> groupViews)
        {
            this.UserId = userId;
            this.UserName = userName;
            this.Items = groupViews;
        }
    }
}
