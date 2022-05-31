#nullable enable
using HotChocolate.Types;
using Main.Core.Entities.SubEntities;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Maps;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Mutations
{
    [ExtendObjectType(Name = "HeadquartersMutations")]
    public class MapsMutationExtension : ObjectTypeExtension
    {
        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor.Name("HeadquartersMutation");
            
            descriptor.Field<MapsResolver>(t => t.DeleteMap(default!, default!))
                .Use<TransactionMiddleware>()
                .Authorize(roles: new[]
                {
                    nameof(UserRoles.Administrator),
                    nameof(UserRoles.Headquarter),
                    nameof(UserRoles.ApiUser)
                })
                .HasWorkspace()
                .Type<Map>()
                .Argument("fileName", a => a.Description("Map file name").Type<NonNullType<StringType>>());

            descriptor.Field<MapsResolver>(t => t.DeleteUserFromMap(default!, default!, default!))
                .Use<TransactionMiddleware>()
                .Authorize(roles: new[]
                {
                    nameof(UserRoles.Administrator),
                    nameof(UserRoles.Headquarter),
                    nameof(UserRoles.Supervisor),
                    nameof(UserRoles.ApiUser)
                })
                .HasWorkspace()
                .Type<Map>()
                .Argument("fileName", a => a.Description("Map file name").Type<NonNullType<StringType>>())
                .Argument("userName", a => a.Type<NonNullType<StringType>>());

            descriptor.Field<MapsResolver>(t => t.AddUserToMap(default!, default!, default!))
                .Use<TransactionMiddleware>()
                .Authorize(roles: new[]
                {
                    nameof(UserRoles.Administrator),
                    nameof(UserRoles.Headquarter),
                    nameof(UserRoles.Supervisor),
                    nameof(UserRoles.ApiUser)
                })
                .HasWorkspace()
                .Type<Map>()
                .Argument("fileName", a => a.Description("Map file name").Type<NonNullType<StringType>>())
                .Argument("userName", a => a.Type<NonNullType<StringType>>());
            
            descriptor.Field<MapsResolver>(t => t.UploadMap(default!, default!))
                .Use<TransactionMiddleware>()
                .Authorize(roles: new[]
                {
                    nameof(UserRoles.Administrator),
                    nameof(UserRoles.Headquarter),
                    nameof(UserRoles.ApiUser)
                })
                .HasWorkspace()
                .Type<Map>()
                .Argument("file", a => a.Type<NonNullType<UploadType>>());
        }
    }
}
