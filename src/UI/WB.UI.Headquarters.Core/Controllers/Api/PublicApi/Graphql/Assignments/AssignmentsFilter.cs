using HotChocolate.Types;
using HotChocolate.Types.Filters;
using WB.Core.BoundedContexts.Headquarters.Assignments;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Assignments
{
    public class AssignmentsFilter : FilterInputType<Core.BoundedContexts.Headquarters.Assignments.Assignment>
    {
        protected override void Configure(IFilterInputTypeDescriptor<Core.BoundedContexts.Headquarters.Assignments.Assignment> descriptor)
        {
            descriptor.BindFieldsExplicitly();
            descriptor.Name("AssignmentsFilter");

            descriptor.Object(x => x.QuestionnaireId)
                .AllowObject<QuestionnaireIdentityFilterInputType>();
            descriptor.Filter(x => x.Archived).BindFiltersExplicitly().AllowEquals();
            descriptor.Filter(x => x.ResponsibleId).BindFiltersExplicitly().AllowEquals().And().AllowIn().And().AllowNotIn().And().AllowNotEquals();
            descriptor.Filter(x => x.WebMode).BindFiltersExplicitly().AllowEquals();
            descriptor.Filter(x => x.ReceivedByTabletAtUtc).BindFiltersExplicitly()
                .AllowEquals().And().AllowGreaterThan().And().AllowLowerThan().And().AllowNotEquals();
        }
    }
}
