using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Headquarters.Models.Api;

namespace WB.UI.Headquarters.API.PublicApi.Models
{
    public class InterviewApiView : BaseApiView
    {
        public InterviewApiView(int page, int pageSize, int totalCount, IEnumerable<InterviewApiItem> interviews, string order)
        {
            this.Offset = page;
            this.TotalCount = totalCount;
            this.Limit = pageSize;
            this.Interviews = interviews;
            this.Order = order;
        }

        public InterviewApiView(AllInterviewsView questionnaireBrowseView)
        {
            if (questionnaireBrowseView == null)
                return;

            this.TotalCount = questionnaireBrowseView.TotalCount;
            this.Interviews = questionnaireBrowseView.Items.Select(
                item => new InterviewApiItem(item.InterviewId, 
                    item.QuestionnaireId, 
                    item.QuestionnaireVersion,
                    item.AssignmentId,
                    item.ResponsibleId,
                    item.ResponsibleName,
                    item.ErrorsCount,
                    (InterviewStatus)Enum.Parse(typeof(InterviewStatus), item.Status),
                    item.LastEntryDateUtc, 
                    item.FeaturedQuestions,
                    item.ReceivedByInterviewerAtUtc));

            this.Limit = questionnaireBrowseView.PageSize;
            this.Offset = questionnaireBrowseView.Page;
        }

        public IEnumerable<InterviewApiItem> Interviews { get; private set; }
    }
}
