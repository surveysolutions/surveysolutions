#nullable enable
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.FileSystem;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.UI.Headquarters.Services.Maps;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Maps
{
    public class MapsResolver
    {
        public IQueryable<MapBrowseItem> GetMaps([Service] IUnitOfWork unitOfWork, [Service]IAuthorizedUser user)
        {
            IQueryable<MapBrowseItem> maps = unitOfWork.Session.Query<MapBrowseItem>();
            
            if (user.IsSupervisor)
            {
                //add all team maps
                var team = unitOfWork.Session.Query<HqUser>()   
                    .Where(x => x.WorkspaceProfile.SupervisorId == user.Id || x.Id == user.Id)
                    .Select(x=> x.UserName);
                
                maps = maps.Where(x => x.Users.Any(z => team.Contains(z.UserName)));
            }
            
            return maps;
        }

        public Task<MapBrowseItem> DeleteMap(string fileName, [Service] IMapStorageService mapStorageService) 
        {
            return mapStorageService.DeleteMap(fileName);
        }

        public MapBrowseItem DeleteUserFromMap(string fileName, string userName,
            [Service] IMapStorageService mapStorageService,
            [Service] IUnitOfWork unitOfWork,
            [Service] IAuthorizedUser authorizedUser)
        {
            if (authorizedUser.IsSupervisor)
            {
                //limit by team
                var team = unitOfWork.Session.Query<HqUser>()
                    .Where(x => x.WorkspaceProfile.SupervisorId == authorizedUser.Id || x.Id == authorizedUser.Id)
                    .Select(x=> x.UserName);
                
                if (authorizedUser.UserName != userName && !team.Contains(userName))
                {
                    throw new GraphQLException(new[]
                    {
                        ErrorBuilder.New()
                            .SetMessage("User has no permissions to perform this action")
                            .SetCode(ErrorCodes.Authentication.NotAuthorized)
                            .Build()
                    });
                }
                
                var mapUsers = unitOfWork.Session.Query<UserMap>()
                    .Where(a => fileName == a.Map.Id);

                if (!mapUsers.Any(z => team.Contains(z.UserName)))
                {
                    throw new GraphQLException(new[]
                    {
                        ErrorBuilder.New()
                            .SetMessage("User has no permissions to perform this action")
                            .SetCode(ErrorCodes.Authentication.NotAuthorized)
                            .Build()
                    });
                }
            }

            return mapStorageService.DeleteMapUserLink(fileName, userName);
        }

        public MapBrowseItem AddUserToMap(string fileName, string userName,
            [Service] IMapStorageService mapStorageService,
            [Service] IUnitOfWork unitOfWork,
            [Service] IAuthorizedUser authorizedUser)
        {
            if (authorizedUser.IsSupervisor)
            {
                //limit by team
                var team = unitOfWork.Session.Query<HqUser>()
                    .Where(x => x.WorkspaceProfile.SupervisorId == authorizedUser.Id || x.Id == authorizedUser.Id)
                    .Select(x=> x.UserName);
                
                if (authorizedUser.UserName != userName && !team.Contains(userName))
                {
                    throw new GraphQLException(new[]
                    {
                        ErrorBuilder.New()
                            .SetMessage("User has no permissions to perform this action")
                            .SetCode(ErrorCodes.Authentication.NotAuthorized)
                            .Build()
                    });
                }
                
                var mapUsers = unitOfWork.Session.Query<UserMap>()
                    .Where(a => fileName == a.Map.Id);

                if (!mapUsers.Any(z => team.Contains(z.UserName)))
                {
                    throw new GraphQLException(new[]
                    {
                        ErrorBuilder.New()
                            .SetMessage("User has no permissions to perform this action")
                            .SetCode(ErrorCodes.Authentication.NotAuthorized)
                            .Build()
                    });
                }
            }
            
            return mapStorageService.AddUserToMap(fileName, userName);
        }

        [RequestSizeLimit(500 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 500 * 1024 * 1024)]
        public async Task<MapBrowseItem[]> UploadMap(IFile file, [Service] IUploadMapsService mapUploadService)
        {
            var uploadMap = await mapUploadService.Upload(file.Name, file.OpenReadStream());
            if (uploadMap.IsSuccess)
                return uploadMap.Maps.ToArray();
            else
                throw new GraphQLException(uploadMap.Errors.Select(message => new Error(message)));
        }
    }
}
