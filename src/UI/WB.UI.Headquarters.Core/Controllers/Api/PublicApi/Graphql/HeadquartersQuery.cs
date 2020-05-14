#nullable enable
using HotChocolate.Types;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Interviews;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Paging;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Questionnaires;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Users;

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
                .Type<ListType<QuestionItemObjectType>>()
                .Argument("id", a => a.Description("Questionnaire id").Type<NonNullType<UuidType>>())
                .Argument("version", a => a.Description("Questionnaire version").Type<NonNullType<LongType>>())
                .Argument("language", a => a.Description("Questionnaire language").Type<StringType?>())
                .UseFiltering<QuestionsFilterType>();

            descriptor.Field<UsersResolver>(x => x.GetViewer(default))
                .Authorize()
                .Type<UserType>().Name("viewer");
        }
    }
}
