using HotChocolate.Types.Filters;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Questionnaires
{
    public class QuestionnaireItemsFilterType: FilterInputType<QuestionnaireCompositeItem>
    {
        protected override void Configure(IFilterInputTypeDescriptor<QuestionnaireCompositeItem> descriptor)
        {
            descriptor.BindFieldsExplicitly();

            descriptor.Name("QuestionnaireItemsFilter");
            
            descriptor.Filter(z => z.QuestionText)
                .Name("title")
                .BindFiltersExplicitly()
                .AllowEquals();
            
            descriptor.Filter(z => z.StataExportCaption)
                .BindFiltersExplicitly()
                .AllowEquals()
                .Name("variable");
            
            descriptor.Filter(x => x.QuestionScope)
                .BindFiltersExplicitly()
                .AllowEquals()
                .Name("scope");

            descriptor.Filter(x => x.Featured)
                .BindFiltersExplicitly()
                .AllowEquals().Name("identifying")
                .Description("Find only identifying questions");
        }
    }
}