using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Factories
{
    public class QuestionnairesList
    {
        public QuestionnairesList()
        {
            this.Items = new List<QuestionnaireListItem>();
        }

        public List<QuestionnaireListItem> Items { get; set; }
        public int TotalCount;
    }
}