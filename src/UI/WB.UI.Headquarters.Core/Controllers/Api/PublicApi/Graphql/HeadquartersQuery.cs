#nullable enable
using HotChocolate.Types;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Assignments;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Interviews;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Maps;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Paging;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Questionnaires;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Users;
using Assignment = WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Assignments.Assignment;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql
{
    public class HeadquartersQuery : ObjectType
    {
        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor.Field<QuestionnairesResolver>(x => x.Questionnaires(default, default, default))
                .Authorize(nameof(UserRoles.Administrator),
                    nameof(UserRoles.Headquarter),
                    nameof(UserRoles.ApiUser))
                .Name("questionnaires")
                .Description("Gets questionnaire details")
                .UseSimplePaging<Questionnaire, QuestionnaireBrowseItem>()
                .Argument("id", a => a.Description("Questionnaire id").Type<UuidType>())
                .Argument("version", a => a.Description("Questionnaire version").Type<LongType>());

            descriptor.Field<InterviewsResolver>(x => x.GetInterviews(default, default))
                .Authorize()
                .UseSimplePaging<Interview, InterviewSummary>()
                .UseFiltering<InterviewsFilterInputType>()
                .UseSorting<InterviewsSortInputType>();

            descriptor.Field<QuestionsResolver>(x => x.Questions(default, default, default, default, default))
                .Authorize()
                .Type<ListType<EntityItemObjectType>>()
                .Argument("id", a => a.Description("Questionnaire id").Type<NonNullType<UuidType>>())
                .Argument("version", a => a.Description("Questionnaire version").Type<NonNullType<LongType>>())
                .Argument("language", a => a.Description("Questionnaire language").Type<StringType?>())
                .UseFiltering<QuestionsFilterType>();

            descriptor.Field<UsersResolver>(x => x.GetViewer(default))
                .Authorize()
                .Type<UserType>().Name("viewer");

            descriptor.Field<MapsResolver>(x => x.GetMaps())
                .Authorize()
                .UseSimplePaging<Map, MapBrowseItem>()
                .UseFiltering<MapsFilterInputType>()
                .UseSorting<MapsSortInputType>();

            descriptor.Field<AssignmentsResolver>(x => x.Assignments(default, default))
                .Authorize()
                .UseSimplePaging<Assignment, Core.BoundedContexts.Headquarters.Assignments.Assignment>()
                .UseFiltering<AssignmentsFilter>();
        }
    }
}
