using HotChocolate.Data.Filters;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Interviews;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Reports.Map
{
    public class GpsAnswerFilterInputType : FilterInputType<GpsAnswerQuery>
    {
        protected override void Configure(IFilterInputTypeDescriptor<GpsAnswerQuery> descriptor)
        {
            descriptor.BindFieldsExplicitly();
            descriptor.Name("MapReportFilter");

            descriptor.Field(x => x.InterviewSummary)
                .Name("interviewFilter")
                .Type<InterviewsFilterInputType>();
        }
    }
}
