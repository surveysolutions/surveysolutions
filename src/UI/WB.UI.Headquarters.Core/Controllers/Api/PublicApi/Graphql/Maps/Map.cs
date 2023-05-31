using System.Linq;
using HotChocolate.Types;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Maps
{ 
    public class Map : ObjectType<MapBrowseItem>
    {
        protected override void Configure(IObjectTypeDescriptor<MapBrowseItem> descriptor)
        {
            descriptor.Name("Map");
            descriptor.Ignore(x => x.FileName);
            descriptor.Field(x => x.Id)
                .Type<NonNullType<StringType>>()
                .Name("fileName")
                .Description("Map file name");
            descriptor.Field(x => x.Size).Description("Size of map in bytes");
            descriptor.Field(x => x.ImportDate)
                .Type<DateTimeType>()
                .Name("importDateUtc")
                .Description("Utc date when map was imported on HQ");
            descriptor.Field(x => x.UploadedBy)
                .Name("uploadedBy")
                .Description("User which uploaded map on HQ");
            descriptor.Field(x => x.Users)
                .Description("List of users assigned to map")
                .Type<NonNullType<ListType<NonNullType<UserMapObjectType>>>>()
                .Resolve(context =>
                {
                    var mapId = context.Parent<MapBrowseItem>().Id;
                    var user = context.Service<IAuthorizedUser>();
                    var unitOfWork = context.Service<IUnitOfWork>();

                    var mapUsers = unitOfWork.Session.Query<UserMap>()
                        .Where(a => mapId == a.Map.Id);
                    
                    if (user.IsSupervisor)
                    {
                        //limit by team
                        var team = unitOfWork.Session.Query<HqUser>()
                            .Where(x => x.WorkspaceProfile.SupervisorId == user.Id || x.Id == user.Id)
                            .Select(x=> x.UserName);
                        
                        mapUsers = mapUsers.Where(z => team.Contains(z.UserName));
                    }

                    return mapUsers.ToList();
                })
                .Type<ListType<UserMapObjectType>>();
            descriptor.Ignore(m => m.HasGeoJson);
            descriptor.Ignore(m => m.GeoJson);
            descriptor.Ignore(m => m.IsPreviewGeoJson);
            descriptor.Ignore(m => m.DuplicateLabels);
        }
    }
}
