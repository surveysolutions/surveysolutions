using System;
using HotChocolate.Types;
using HotChocolate.Types.Relay;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Interviews;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Questionnaires;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql
{
    public class HeadquartersQuery : ObjectType
    {
        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor.Field<InterviewsResolver>(x => x.GetInterviews(default, default, default, default))
                .UseSelection()
                .Argument("skip", a => a.Type<IntType>())
                .Argument("take", a => a.Type<IntType>())
                .Authorize()
                .Type<NonNullType<ListType<InterviewSummaryObjectType>>>()
                .UsePaging<InterviewSummaryObjectType>()
                .UseFiltering<InterviewsFilerInputType>()
                .UseSorting<InterviewsSortInputType>();

            descriptor.Field<QuestionsResolver>(x => x.Questions(default, default, default, default))
                .Authorize()
                .Type<NonNullType<ListType<QuestionItemObjectType>>>()
                .Argument("id", a => a.Description("Questionnaire id").Type<NonNullType<UuidType>>())
                .Argument("version", a => a.Description("Questionnaire version").Type<NonNullType<LongType>>())
                .Argument("language", a => a.Description("Questionnaire language").Type<StringType>())
                .UseFiltering<QuestionsFilterType>();
        }
    }
}
