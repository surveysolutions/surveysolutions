using HotChocolate.Types;
using WB.Core.SharedKernels.DataCollection;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Questionnaires
{
    public class CategoricalOptionType : ObjectType<CategoricalOption>
    {
        protected override void Configure(IObjectTypeDescriptor<CategoricalOption> descriptor)
        {
            descriptor.Name("CategoricalOption");

            descriptor.Field(x => x.Title).Type<NonNullType<StringType>>();
        }
    }
}
