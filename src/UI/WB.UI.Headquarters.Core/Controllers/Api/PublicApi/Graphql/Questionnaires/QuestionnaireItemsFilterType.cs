using HotChocolate.Data.Filters;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Questionnaires
{
    public class QuestionnaireItemsFilterType: FilterInputType<QuestionnaireCompositeItem>
    {
        protected override void Configure(IFilterInputTypeDescriptor<QuestionnaireCompositeItem> descriptor)
        {
            descriptor.BindFieldsExplicitly();
            descriptor.Name("QuestionnaireItemsFilter");
            
            descriptor.Field(z => z.QuestionText)
                .Name("title");
            
            descriptor.Field(z => z.StataExportCaption)
                .Name("variable");
            
            descriptor.Field(x => x.QuestionScope)
                .Name("scope");

            descriptor.Field(x => x.Featured)
                .Name("identifying")
                .Description("Find only identifying entities");

            descriptor.Field(x => x.UsedInReporting)
                .Name("exposed")
                .Description("Is this entity exposed");
        }
    }
}
