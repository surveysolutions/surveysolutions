using HotChocolate.Types;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Questionnaires
{
    public class Translation : ObjectType<WB.Core.SharedKernels.SurveySolutions.Documents.Translation>
    {
        protected override void Configure(IObjectTypeDescriptor<Core.SharedKernels.SurveySolutions.Documents.Translation> descriptor)
        {
            base.Configure(descriptor);
            descriptor.BindFieldsExplicitly();
            
            descriptor.Field(x => x.Id)
                .Type<NonNullType<UuidType>>();
            descriptor.Field(x => x.Name)
                .Type<NonNullType<StringType>>();
        }
    }
}
