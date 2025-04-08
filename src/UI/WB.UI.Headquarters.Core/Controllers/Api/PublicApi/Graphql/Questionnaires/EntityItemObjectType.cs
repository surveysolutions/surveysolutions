#nullable enable
using HotChocolate.Types;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;


namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Questionnaires
{
    public class EntityItemObjectType : ObjectType<QuestionnaireCompositeItem>
    {
        protected override void Configure(IObjectTypeDescriptor<QuestionnaireCompositeItem> descriptor)
        {
            descriptor.BindFieldsExplicitly();

            descriptor.Name("Entity");
            
            descriptor.Field(x => x.Featured)
                .Name("identifying")
                .Type<BooleanType>();
            
            descriptor.Field(x => x.VariableLabel)
                .Name("label")
                .Type<StringType>();
            
            descriptor.Field("options")
                .Name("options")
                .Resolve(async context =>
                {
                    var loader = context.DataLoader<QuestionnaireItemOptionsDataLoader>();
                    return await loader.LoadAsync(context.Parent<QuestionnaireCompositeItem>().Id, context.RequestAborted);
                })
                .Type<NonNullType<ListType<NonNullType<CategoricalOptionType>>>>();
            
            descriptor.Field(x => x.QuestionText)
                .Description("Question text. May contain html tags.")
                .Type<StringType>();

            descriptor.Field(x => x.QuestionScope)
                .Name("scope")
                .Type<EnumType<QuestionScope>>();
            
            descriptor.Field(x => x.QuestionType)
                .Name("type")
                .Type<QuestionTypeObjectType>();

            descriptor.Field(x => x.StataExportCaption)
                .Name("variable")
                .Type<StringType>();

            descriptor.Field(x => x.VariableType)
                .Name("variableType")
                .Type<VariableTypeObjectType>();
        }
    }
}
