using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire.BrowseItem;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models.Api
{
    public class QuestionnaireApiView : BaseApiView
    {
        public QuestionnaireApiView(int page, int pageSize, int totalCount, IEnumerable<QuestionnaireApiItem> questionnaire, string order)
        {
            this.Offset = page;
            this.TotalCount = totalCount;
            this.Limit = pageSize;
            this.Questionnaires = questionnaire;
            this.Order = order;
        }

        public QuestionnaireApiView(QuestionnaireBrowseView questionnaireBrowseView)
        {
            if (questionnaireBrowseView == null)
                return;

            this.Offset = questionnaireBrowseView.Page;
            this.TotalCount = questionnaireBrowseView.TotalCount;
            this.Limit = questionnaireBrowseView.PageSize.GetValueOrDefault();
            this.Questionnaires = questionnaireBrowseView.Items.Select(
                    item => new QuestionnaireApiItem(item.QuestionnaireId, item.Version, item.Title, item.LastEntryDate));
            this.Order = questionnaireBrowseView.Order;
        }
        
        public IEnumerable<QuestionnaireApiItem> Questionnaires { get; private set; }
    }
}