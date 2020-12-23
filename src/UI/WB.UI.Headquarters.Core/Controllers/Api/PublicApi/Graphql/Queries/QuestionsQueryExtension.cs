#nullable enable
using HotChocolate.Types;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Questionnaires;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Queries
{
    public class QuestionsQueryExtension : ObjectTypeExtension
    {
        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor.Name("HeadquartersQuery");
            
            descriptor.Field<QuestionsResolver>(x => x.Questions(default, default, default!, default!, default!))
                .Authorize()
                .HasWorkspace()
                .Type<ListType<EntityItemObjectType>>()
                .Argument("id", a => a.Description("Questionnaire id").Type<NonNullType<UuidType>>())
                .Argument("version", a => a.Description("Questionnaire version").Type<NonNullType<LongType>>())
                .Argument("language", a => a.Description("Questionnaire language").Type<StringType?>())
                .UseFiltering<QuestionsFilterType>();
        }
    }
}
