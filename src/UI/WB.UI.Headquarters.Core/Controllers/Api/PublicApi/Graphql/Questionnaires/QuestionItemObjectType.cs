using HotChocolate.Types;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Questionnaires
{
    public class QuestionItemObjectType : ObjectType<QuestionnaireCompositeItem>
    {
        protected override void Configure(IObjectTypeDescriptor<QuestionnaireCompositeItem> descriptor)
        {
            descriptor.BindFieldsExplicitly();

            descriptor.Name("Question");
            
            descriptor.Field(x => x.QuestionText)
                .Type<NonNullType<StringType>>();

            descriptor.Field(x => x.StatExportCaption)
                .Name("variable")
                .Type<NonNullType<StringType>>();

            descriptor.Field(x => x.QuestionScope)
                .Name("scope")
                .Type<NonNullType<EnumType<QuestionScope>>>();

            descriptor.Field(x => x.Featured)
                .Name("identifying")
                .Type<NonNullType<BooleanType>>();
        }
    }
}
