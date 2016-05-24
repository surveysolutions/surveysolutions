using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models.Api
{
    public class InterviewApiView : BaseApiView
    {
        public InterviewApiView(int page, int pageSize, int totalCount, IEnumerable<InterviewApiItem> questionnaire, string order)
        {
            this.Offset = page;
            this.TotalCount = totalCount;
            this.Limit = pageSize;
            this.Interviews = questionnaire;
            this.Order = order;
        }

        public InterviewApiView(AllInterviewsView questionnaireBrowseView)
        {
            if (questionnaireBrowseView == null)
                return;

            this.TotalCount = questionnaireBrowseView.TotalCount;
            this.Interviews = questionnaireBrowseView.Items.Select(
                item => new InterviewApiItem(item.InterviewId, item.QuestionnaireId, item.QuestionnaireVersion,
                    item.ResponsibleId, item.ResponsibleName, item.HasErrors, item.Status, item.LastEntryDate, 
                    item.FeaturedQuestions));


            this.Limit = questionnaireBrowseView.PageSize;
            this.Offset = questionnaireBrowseView.Page;
            /*this.Order = questionnaireBrowseView.Order;*/
        }

        public IEnumerable<InterviewApiItem> Interviews { get; private set; }
    }
}