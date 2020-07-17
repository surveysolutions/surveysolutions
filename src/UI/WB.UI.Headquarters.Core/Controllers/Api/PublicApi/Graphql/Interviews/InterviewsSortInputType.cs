using HotChocolate.Types.Sorting;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Interviews
{
    public class InterviewsSortInputType : SortInputType<InterviewSummary>
    {
        protected override void Configure(ISortInputTypeDescriptor<InterviewSummary> descriptor)
        {
            descriptor.BindFieldsExplicitly();
            descriptor.Name("InterviewSort");
            descriptor.Sortable(x => x.Key);
            descriptor.Sortable(x => x.CreatedDate);
            descriptor.Sortable(x => x.UpdateDate);
            descriptor.Sortable(x => x.ResponsibleName);
            descriptor.Sortable(x => x.ResponsibleRole);
            descriptor.Sortable(x => x.AssignmentId);
            descriptor.Sortable(x => x.ErrorsCount);
            descriptor.Sortable(x => x.Status);
            descriptor.Sortable(x => x.ReceivedByInterviewerAtUtc);
            descriptor.Sortable(x => x.QuestionnaireId);
            descriptor.Sortable(x => x.QuestionnaireVersion);
            descriptor.Sortable(x => x.SummaryId).Name("id");
        }
    }
}
