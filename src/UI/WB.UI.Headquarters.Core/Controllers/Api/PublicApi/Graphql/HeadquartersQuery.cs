#nullable enable
using HotChocolate.Types;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Interviews;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Maps;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Paging;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Questionnaires;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Users;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql
{
    public class HeadquartersQuery : ObjectType
    {
        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor.Field<InterviewsResolver>(x => x.GetInterviews(default, default))
                .Authorize()
                .UseSimplePaging<Interview, InterviewSummary>()
                .UseFiltering<InterviewsFilterInputType>()
                .UseSorting<InterviewsSortInputType>();

            descriptor.Field<QuestionsResolver>(x => x.Questions(default, default, default, default, default))
                .Authorize()
                .Type<ListType<QuestionItemObjectType>>()
                .Argument("id", a => a.Description("Questionnaire id").Type<UuidType>())
                .Argument("version", a => a.Description("Questionnaire version").Type<LongType>())
                .Argument("language", a => a.Description("Questionnaire language").Type<StringType?>())
                .UseFiltering<QuestionsFilterType>();

            descriptor.Field<UsersResolver>(x => x.GetViewer(default))
                .Authorize()
                .Type<UserType>().Name("viewer");

            descriptor.Field<MapsResolver>(x => x.GetMaps(default))
                .Authorize()
                .UseSimplePaging<Map, MapBrowseItem>()
                .UseFiltering<MapsFilterInputType>()
                .UseSorting<MapsSortInputType>();

            descriptor.Field<MapsResolver>(x => x.GetMap(default, default))
                .Authorize()
                .Type<Map>()
                .Argument("id", a => a.Description("Map file name").Type<StringType>())
                .Name("map");
        }
    }
}
