using HotChocolate.Data.Sorting;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Interviews
{
    public class InterviewsSortInputType : SortInputType<InterviewSummary>
    {
        protected override void Configure(ISortInputTypeDescriptor<InterviewSummary> descriptor)
        {
            descriptor.BindFieldsExplicitly();
            descriptor.Name("InterviewSort");
            descriptor.Field(x => x.Key);
            descriptor.Field(x => x.CreatedDate);
            descriptor.Field(x => x.UpdateDate).Name("updateDateUtc");
            descriptor.Field(x => x.ResponsibleName);
            descriptor.Field(x => x.ResponsibleRole);
            descriptor.Field(x => x.AssignmentId);
            descriptor.Field(x => x.ErrorsCount);
            descriptor.Field(x => x.Status);
            descriptor.Field(x => x.ReceivedByInterviewerAtUtc);
            descriptor.Field(x => x.QuestionnaireId);
            descriptor.Field(x => x.QuestionnaireVersion);
            descriptor.Field(x => x.SummaryId).Name("id");
            descriptor.Field(x => x.NotAnsweredCount);
        }
    }
}
