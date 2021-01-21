using System;
using HotChocolate.Data.Filters;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Assignments
{
    public class QuestionnaireIdentityFilterInputType : FilterInputType<QuestionnaireIdentity>
    {
        protected override void Configure(IFilterInputTypeDescriptor<QuestionnaireIdentity> descriptor)
        {
            descriptor.Name("QuestionnaireIdentity");
            descriptor.BindFieldsExplicitly();
            descriptor.Field(x => x.QuestionnaireId)
                .Name("id");
                
            descriptor.Field(x => x.Version).Name("version");
        }
    }
}
