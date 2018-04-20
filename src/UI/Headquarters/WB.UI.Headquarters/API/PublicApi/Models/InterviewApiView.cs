using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.Api;

namespace WB.UI.Headquarters.API.PublicApi.Models
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
                    item.ResponsibleId, item.ResponsibleName, item.ErrorsCount, (InterviewStatus)Enum.Parse(typeof(InterviewStatus), item.Status), item.LastEntryDateUtc, 
                    item.FeaturedQuestions));


            this.Limit = questionnaireBrowseView.PageSize;
            this.Offset = questionnaireBrowseView.Page;
            /*this.Order = questionnaireBrowseView.Order;*/
        }

        public IEnumerable<InterviewApiItem> Interviews { get; private set; }
    }
}
