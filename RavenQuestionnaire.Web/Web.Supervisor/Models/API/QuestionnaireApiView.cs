using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire.BrowseItem;

namespace Web.Supervisor.Models.API
{
    public class QuestionnaireApiView
    {
        public QuestionnaireApiView(int page, int pageSize, int totalCount, IEnumerable<QuestionnaireApiItem> questionnaire, string order)
        {
            this.Page = page;
            this.TotalCount = totalCount;
            this.PageSize = pageSize;
            this.Questionnaires = questionnaire;
            this.Order = order;
        }

        public QuestionnaireApiView(QuestionnaireBrowseView questionnaireBrowseView)
        {
            this.Page = questionnaireBrowseView.Page;
            this.TotalCount = questionnaireBrowseView.TotalCount;
            this.PageSize = questionnaireBrowseView.PageSize;
            this.Questionnaires = questionnaireBrowseView.Items.Select(
                    item => new QuestionnaireApiItem(item.QuestionnaireId, item.Version, item.Title, item.LastEntryDate));
            this.Order = questionnaireBrowseView.Order;
        }

        public IEnumerable<QuestionnaireApiItem> Questionnaires { get; private set; }

        public string Order { get; private set; }

        public int PageSize { get; private set; }

        public int TotalCount { get; private set; }

        public int Page { get; private set; }
        
    }
}