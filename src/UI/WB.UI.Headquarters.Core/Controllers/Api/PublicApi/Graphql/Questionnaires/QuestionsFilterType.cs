using HotChocolate.Data.Filters;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Questionnaires
{
    public class QuestionsFilterType : FilterInputType<QuestionnaireCompositeItem>
    {
        protected override void Configure(IFilterInputTypeDescriptor<QuestionnaireCompositeItem> descriptor)
        {
            descriptor.BindFieldsExplicitly();

            descriptor.Name("QuestionFilter");
            
            //descriptor.Field(z => z.QuestionText);
            
            descriptor.Field(z => z.StataExportCaption)
                      .Name("variable");
            
            descriptor.Field(x => x.QuestionScope)
                      .Name("scope");

            descriptor.Field(x => x.Featured)
                      .Name("identifying")
                      .Description("Find only identifying questions");
        }
    }
}
