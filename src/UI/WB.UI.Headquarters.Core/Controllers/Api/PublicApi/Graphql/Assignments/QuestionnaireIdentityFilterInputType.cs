using HotChocolate.Types.Filters;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Assignments
{
    public class QuestionnaireIdentityFilterInputType : FilterInputType<QuestionnaireIdentity>
    {
        protected override void Configure(IFilterInputTypeDescriptor<QuestionnaireIdentity> descriptor)
        {
            descriptor.Name("QuestionnaireIdentity");
            descriptor.BindFieldsExplicitly();
            descriptor.Filter(x => x.QuestionnaireId).BindFiltersExplicitly().Name("id").AllowEquals().And().AllowNotEquals();
            descriptor.Filter(x => x.Version).Name("version");
        }
    }
}
