using System;
using System.Collections.Generic;
using System.Linq;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using RavenQuestionnaire.Core.Views.Statistics;

namespace RavenQuestionnaire.Core.Views.User
{
    public class InterviewerView
    {
        public int PageSize
        {
            get;
            private set;
        }

        public int Page
        {
            get;
            private set;
        }

        public int TotalCount { get; private set; }
        public string UserName { get; private set; }

        public List<InterviewerItemView> Items
        {
            get;
            private set;
        }

        public Dictionary<Guid, string> FeaturedHeaders { get; private set; }

        public InterviewerView(int page, int pageSize, int totalCount, string UserName, List<CompleteQuestionnaireBrowseItem> questionnaires)
        {
            this.Page = page;
            this.TotalCount = totalCount;
            this.PageSize = pageSize;
            this.Items = new List<InterviewerItemView>();
            FeaturedHeaders = new Dictionary<Guid, string>();

            foreach (var question in questionnaires.SelectMany(completeQuestionnaireBrowseItem => completeQuestionnaireBrowseItem.FeaturedQuestions))
            {
                if (!FeaturedHeaders.ContainsKey(question.PublicKey))
                {
                    FeaturedHeaders.Add(question.PublicKey, question.QuestionText);
                }
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
