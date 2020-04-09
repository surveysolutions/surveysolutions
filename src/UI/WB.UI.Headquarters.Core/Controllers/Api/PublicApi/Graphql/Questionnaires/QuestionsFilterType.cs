using System;
using HotChocolate.Types.Filters;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Questionnaires
{
    public class QuestionsFilterType : FilterInputType<QuestionnaireCompositeItem>
    {
        protected override void Configure(IFilterInputTypeDescriptor<QuestionnaireCompositeItem> descriptor)
        {
            descriptor.BindFieldsExplicitly();

            descriptor.Name("QuestionFilter");
            
            descriptor.Filter(z => z.QuestionText)
                      .BindFiltersExplicitly()
                      .AllowEquals();
            
            descriptor.Filter(z => z.StatExportCaption)
                      .BindFiltersExplicitly()
                      .AllowEquals().Name("variable");
            
            descriptor.Filter(x => x.QuestionScope)
                      .BindFiltersExplicitly()
                      .AllowEquals().Name("scope");

            descriptor.Filter(x => x.Featured)
                      .BindFiltersExplicitly()
                      .AllowEquals().Name("identifying")
                      .Description("Find only identifying questions");

        }
    }
}
