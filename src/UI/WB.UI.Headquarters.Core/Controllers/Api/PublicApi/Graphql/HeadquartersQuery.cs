#nullable enable
using HotChocolate.Types;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;
using WB.Core.BoundedContexts.Headquarters.Workspaces.Mappings;
using WB.Infrastructure.Native.Workspaces;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Assignments;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Interviews;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Maps;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Paging;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Questionnaires;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Users;
using Assignment = WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Assignments.Assignment;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql
{
    public static class SchemaExtensions
    {
        public static IObjectFieldDescriptor HasWorkspace(this IObjectFieldDescriptor descriptor)
        {
            return descriptor.Argument("workspace",
                a => a.Description("Workspace name").Type<NonNullType<StringType>>()
                    .DefaultValue(WorkspaceConstants.DefaultWorkspaceName));
        }
    }
    
    public class HeadquartersQuery : ObjectType
    {
        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor
                .Field<QuestionnairesResolver>(x => x.Questionnaires(default, default, default))
                .HasWorkspace()
                .Authorize(nameof(UserRoles.Administrator),
                    nameof(UserRoles.Headquarter),
                    nameof(UserRoles.ApiUser))
                .Name("questionnaires")
                .Description("Gets questionnaire details")
                .UseSimplePaging<Questionnaire, QuestionnaireBrowseItem>()

                .Argument("id", a => a.Description("Questionnaire id").Type<UuidType>())
                .Argument("version", a => a.Description("Questionnaire version").Type<LongType>());

            descriptor.Field<InterviewsResolver>(x => x.GetInterviews(default, default))
                .HasWorkspace()
                .Authorize()
                .UseSimplePaging<Interview, InterviewSummary>()
                .UseFiltering<InterviewsFilterInputType>()
                .UseSorting<InterviewsSortInputType>();

            descriptor.Field<QuestionsResolver>(x => x.Questions(default, default, default, default, default))
                .HasWorkspace()
                .Authorize()
                .Type<ListType<EntityItemObjectType>>()
                .Argument("id", a => a.Description("Questionnaire id").Type<NonNullType<UuidType>>())
                .Argument("version", a => a.Description("Questionnaire version").Type<NonNullType<LongType>>())
                .Argument("language", a => a.Description("Questionnaire language").Type<StringType?>())
                .UseFiltering<QuestionsFilterType>();

            descriptor.Field<QuestionnaireItemResolver>(x => x.QuestionnaireItems(default, default, default, default, default))
                .HasWorkspace()
                .Authorize()
                .Type<ListType<QuestionnaireItemObjectType>>()
                .Argument("id", a => a.Description("Questionnaire id").Type<NonNullType<UuidType>>())
                .Argument("version", a => a.Description("Questionnaire version").Type<NonNullType<LongType>>())
                .Argument("language", a => a.Description("Questionnaire language").Type<StringType?>())
                .UseFiltering<QuestionnaireItemsFilterType>();

            descriptor.Field<UsersResolver>(x => x.GetViewer(default))
                .Authorize()
                .Type<UserType>().Name("viewer");

            descriptor.Field<MapsResolver>(x => x.GetMaps())
                .Authorize()
                .UseSimplePaging<Map, MapBrowseItem>()
                .UseFiltering<MapsFilterInputType>()
                .UseSorting<MapsSortInputType>()
                .HasWorkspace();

            descriptor.Field<AssignmentsResolver>(x => x.Assignments(default, default))
                .Authorize()
                .UseSimplePaging<Assignment, Core.BoundedContexts.Headquarters.Assignments.Assignment>()
                .UseFiltering<AssignmentsFilter>()
                .HasWorkspace();
        }
    }
}
