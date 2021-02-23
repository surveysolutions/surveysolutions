using HotChocolate.Data.Filters;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Filters
{
    public class IdentifyEntityValueFilterInput : FilterInputType<IdentifyEntityValue>
    {
        protected override void Configure(IFilterInputTypeDescriptor<IdentifyEntityValue> descriptor)
        {
            descriptor.BindFieldsExplicitly();
            descriptor.Name("IdentifyEntityValueFilter");

            descriptor.Field(x => x.AnswerCode)
                .Description("Code of answer for categorical question");
            descriptor.Field(x => x.Value)
                .Description("Answer value, supports case sensitive operations");
            descriptor.Field(x => x.ValueLowerCase)
                .Description("Answer value in lower case, supports case insensitive operations");
            descriptor.Field(x => x.Entity)
                .Description("Question or variable entity");

            descriptor.Field(x => x.IsEnabled)
                .Description("Shows if this value enabled");
        }
    }
}
