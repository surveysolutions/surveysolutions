using System;
using System.Linq;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;

namespace RavenQuestionnaire.Core.Views.User
{
    public class InterviewerView
    {
        public int PageSize { get; private set; }

        public int Page { get; private set; }

        public int TotalCount { get; private set; }

        public string UserName { get; private set; }

        public string UserId { get; private set; }

        public List<InterviewerItemView> Items { get; private set;}

        public string Order {
            get { return _order; }
            set { _order = value; }
        }
        private string _order = string.Empty;

        public Dictionary<Guid, string> FeaturedHeaders { get; private set; }

        public InterviewerView(int page, int pageSize, int totalCount, string userName, string userId, List<CompleteQuestionnaireBrowseItem> questionnaires)
        {
            this.Page = page;
            this.TotalCount = totalCount;
            this.PageSize = pageSize;
            this.UserId = userId;
            this.UserName = userName;
            this.Items = new List<InterviewerItemView>();
            FeaturedHeaders = new Dictionary<Guid, string>();
            var helper= new Dictionary<Guid, string>();
            foreach (var question in questionnaires.SelectMany(completeQuestionnaireBrowseItem => completeQuestionnaireBrowseItem.FeaturedQuestions.Where(t=>!string.IsNullOrEmpty(t.QuestionText))))
            {
                if (!helper.ContainsKey(question.PublicKey))
                {
                    helper.Add(question.PublicKey, question.QuestionText);
                }
            }
            foreach (var r in helper.OrderBy(t=>t.Key))
            {
                FeaturedHeaders.Add(r.Key, r.Value);
            }
            foreach (var completeQuestionnaireBrowseItem in questionnaires)
            {
                var item = new InterviewerItemView(completeQuestionnaireBrowseItem, FeaturedHeaders);
                Items.Add(item);
            }

        }
        public IEnumerable<CompleteQuestionnaireBrowseItem> Surveys { get; set; }
    }
}
