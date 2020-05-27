﻿using System.Linq;
using HotChocolate.Types;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Maps
{ 
    public class Map : ObjectType<MapBrowseItem>
    {
        protected override void Configure(IObjectTypeDescriptor<MapBrowseItem> descriptor)
        {
            descriptor.Name("Map");
            descriptor.Ignore(x => x.FileName);
            descriptor.Field(x => x.Id).Type<NonNullType<StringType>>().Name("fileName").Description("Map file name");
            descriptor.Field(x => x.Size).Description("Size of map in bytes");
            descriptor.Field(x => x.ImportDate).Description("Utc date when map was imported on HQ");
            descriptor.Field(x => x.Users)
                .Description("List of users assigned to map")
                .Type<NonNullType<ListType<NonNullType<UserMapObjectType>>>>()
                .Resolver(context =>
                {
                    var mapId = context.Parent<MapBrowseItem>().Id;

                    return context.Service<IUnitOfWork>().Session.Query<UserMap>()
                        .Where(a => mapId == a.Map.Id)
                        .ToList();
                })
                .Type<ListType<UserMapObjectType>>();
        }
    }
}
