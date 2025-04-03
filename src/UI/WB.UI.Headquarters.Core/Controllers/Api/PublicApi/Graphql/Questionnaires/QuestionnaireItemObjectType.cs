#nullable enable
using HotChocolate.Types;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;


namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Questionnaires
{
    public class QuestionnaireItemObjectType : ObjectType<QuestionnaireCompositeItem>
    {
        protected override void Configure(IObjectTypeDescriptor<QuestionnaireCompositeItem> descriptor)
        {
            descriptor.BindFieldsExplicitly();

            descriptor.Name("QuestionnaireItem");
            
            descriptor.Field(x => x.EntityType)
                .Type<EntityTypeObjectType>();

            descriptor.Field(x => x.QuestionText)
                .Name("title")
                .Description("Question text or Variable label. May contain html tags.")
                .Type<StringType>();

            descriptor.Field(x => x.StataExportCaption)
                .Name("variable")
                .Type<StringType>();

            descriptor.Field(x => x.QuestionScope)
                .Name("scope")
                .Type<EnumType<QuestionScope>>();

            descriptor.Field(x => x.VariableLabel)
                .Name("label")
                .Type<StringType>();

            descriptor.Field(x => x.QuestionType)
                .Name("type")
                .Type<QuestionTypeObjectType>();

            descriptor.Field(x => x.VariableType)
                .Name("variableType")
                .Type<VariableTypeObjectType>();

            descriptor.Field(x => x.Featured)
                .Name("identifying")
                .Type<BooleanType>();

            descriptor.Field(x => x.IncludedInReportingAtUtc)
                .Name("includedInReportingAtUtc")
                .Type<DateTimeType>();

            descriptor.Field("options")
                .Name("options")
                .Resolve(async context =>
                {
                    var loader = context.DataLoader<QuestionnaireItemOptionsDataLoader>();
                    return await loader.LoadAsync(context.Parent<QuestionnaireCompositeItem>().Id, context.RequestAborted);
                })
                .Type<NonNullType<ListType<NonNullType<CategoricalOptionType>>>>();
        }
    }
}
