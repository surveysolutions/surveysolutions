using System;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;

namespace RavenQuestionnaire.Core.Views.User
{
    public class InterviewerInputModel
    {
        public InterviewerInputModel(string id)
        {
            UserId = id;
        }

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
        private int _pageSize = 20;

        public string UserId { get; private set; }

        public Func<CompleteQuestionnaireBrowseItem, bool> Expression
        {
            get
            {
                return q => (q.Responsible == null ? false : q.Responsible.Id == UserId);
            }
        }
    }
}
