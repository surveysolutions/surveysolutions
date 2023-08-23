using HotChocolate.Types;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Maps
{
    public class UserMapObjectType : ObjectType<UserMap>
    {
        protected override void Configure(IObjectTypeDescriptor<UserMap> descriptor)
        {
            descriptor.BindFieldsExplicitly();
            descriptor.Name("UserMap");
            
            descriptor.Field(x => x.UserName)
                .Type<StringType>()
                .Description("Name of the user assigned to map");
        }
    }
}
