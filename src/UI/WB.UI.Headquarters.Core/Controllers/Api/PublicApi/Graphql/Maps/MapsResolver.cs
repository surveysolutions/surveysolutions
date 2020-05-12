using System.Linq;
using HotChocolate;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Maps
{
    public class MapsResolver
    {
        public IQueryable<MapBrowseItem> GetMaps([Service] IUnitOfWork unitOfWork)
        {
            unitOfWork.DiscardChanges();

            return unitOfWork.Session.Query<MapBrowseItem>();
        }
    }
}
