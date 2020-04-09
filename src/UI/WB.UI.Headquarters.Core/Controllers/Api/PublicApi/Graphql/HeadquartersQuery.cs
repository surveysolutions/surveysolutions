#nullable enable
using HotChocolate.Types;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Interviews;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Paging;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Questionnaires;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql
{
    public class HeadquartersQuery : ObjectType
    {
        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor.Field<InterviewsResolver>(x => x.GetInterviews(default, default))
                .Authorize()
                .UseSimplePaging<InterviewSummaryObjectType, InterviewSummary>()
                .UseFiltering<InterviewsFilterInputType>()
                .UseSorting<InterviewsSortInputType>();

            descriptor.Field<QuestionsResolver>(x => x.Questions(default, default, default, default))
                .Authorize()
                .Type<NonNullType<ListType<QuestionItemObjectType>>>()
                .Argument("id", a => a.Description("Questionnaire id").Type<UuidType>())
                .Argument("version", a => a.Description("Questionnaire version").Type<LongType>())
                .Argument("language", a => a.Description("Questionnaire language").Type<StringType?>())
                .UseFiltering<QuestionsFilterType>();
        }
    }
}
