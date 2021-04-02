#nullable enable
using HotChocolate.Types;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Paging;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Questionnaires;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Queries
{
    public class QuestionnairesQueryExtension : ObjectTypeExtension
    {
        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor.Name("HeadquartersQuery");
            
            descriptor
                .Field<QuestionnairesResolver>(x => x.Questionnaires(default, default, default))
                .Authorize(/*nameof(UserRoles.Administrator),
                    nameof(UserRoles.Headquarter),
                    nameof(UserRoles.ApiUser)*/)//if roles are specified policy is not verified
                .HasWorkspace()
                .Name("questionnaires")
                .Description("Gets questionnaire details")
                .UseSimplePaging<Questionnaire, QuestionnaireBrowseItem>()
                .Argument("id", a => a.Description("Questionnaire id").Type<UuidType>())
                .Argument("version", a => a.Description("Questionnaire version").Type<LongType>());
        }
    }
}
