using System;
using HotChocolate.Data.Filters;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Assignments
{
    public class AssignmentsFilter : FilterInputType<Core.BoundedContexts.Headquarters.Assignments.Assignment>
    {
        protected override void Configure(IFilterInputTypeDescriptor<Core.BoundedContexts.Headquarters.Assignments.Assignment> descriptor)
        {
            descriptor.BindFieldsExplicitly();
            descriptor.Name("AssignmentsFilter");

            descriptor.Field(x => x.Id);
            descriptor.Field(x => x.QuestionnaireId).Type<QuestionnaireIdentityFilterInputType>();
            descriptor.Field(x => x.Archived);
            descriptor.Field(x => x.ResponsibleId);
            descriptor.Field(x => x.WebMode);
            descriptor.Field(x => x.ReceivedByTabletAtUtc);
        }
    }
}
