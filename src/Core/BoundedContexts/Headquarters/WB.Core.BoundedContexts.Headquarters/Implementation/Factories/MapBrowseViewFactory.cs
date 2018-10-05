using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Maps;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;
using WB.Core.Infrastructure.PlainStorage;
using WB.Infrastructure.Native.Utils;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Factories
{
    public class MapBrowseViewFactory : IMapBrowseViewFactory
    {
        private readonly IPlainStorageAccessor<MapBrowseItem> mapBrowseItemReader;
        private readonly IPlainStorageAccessor<UserMap> userMapReader;

        public MapBrowseViewFactory(IPlainStorageAccessor<MapBrowseItem> mapBrowseItemeRader, 
            IPlainStorageAccessor<UserMap> userMapReader)
        {
            this.mapBrowseItemReader = mapBrowseItemeRader;
            this.userMapReader = userMapReader;
        }

        public MapsView Load(MapsInputModel input)
        {
            return this.mapBrowseItemReader.Query(queryable =>
            {
                IQueryable<MapBrowseItem> query = queryable;

                
                if (!string.IsNullOrEmpty(input.SearchBy))
                {
                    var filterLowerCase = input.SearchBy.ToLower();
                    query = query.Where(x => x.FileName.ToLower().Contains(filterLowerCase));
                }

                
                var queryResult = query.OrderUsingSortExpression(input.Order);

                IQueryable<MapBrowseItem> pagedResults = queryResult;

                if (input.PageSize > 0)
                {
                    pagedResults = queryResult.Skip((input.Page - 1) * input.PageSize).Take(input.PageSize);
                }
                
                return new MapsView(){Page = input.Page , PageSize = input.PageSize , TotalCount = queryResult.Count(), Items = pagedResults.ToList() };
            });
            
        }

        public MapUsersView Load(MapUsersInputModel input)
        {
            return this.userMapReader.Query(queryable =>
            {
                IQueryable<UserMap> query = queryable;

                query = query.Where(x => x.Map == input.MapName);

                if (!string.IsNullOrEmpty(input.SearchBy))
                {
                    query = query.Where(x=> x.UserName.ToLower().Contains(input.SearchBy.ToLower()));
                }

                var queryResult = query.OrderUsingSortExpression(input.Order);

                IQueryable<UserMap> pagedResults = queryResult;

                if (input.PageSize > 0)
                {
                    pagedResults = queryResult.Skip((input.Page - 1) * input.PageSize).Take(input.PageSize);
                }

                return new MapUsersView() { Page = input.Page, PageSize = input.PageSize, TotalCount = queryResult.Count(), Items = pagedResults.Select(x => x.UserName).ToList() };
            });
        }

        public UserMapsView Load(UserMapsInputModel input)
        {
            int totalCount = 0;

            var users= this.userMapReader.Query(queryable =>
            {
                IQueryable<UserMap> query = queryable;

                if (!string.IsNullOrEmpty(input.SearchBy))
                {
                    var searchByToLower = input.SearchBy.ToLower();
                    query = query.Where(x => x.UserName.ToLower().Contains(searchByToLower));
                }
                
                var queryResult = query.OrderUsingSortExpression(input.Order);

                var result = queryResult.Select(x => x.UserName).Distinct();
                totalCount = result.Count();

                if (input.PageSize > 0)
                {
                    result = result.Skip((input.Page - 1) * input.PageSize).Take(input.PageSize);
                }
                
                return result.ToList();
            });
            
            return new UserMapsView()
            {
                Page = input.Page,
                PageSize = input.PageSize,
                TotalCount = totalCount,
                Items = users.Select(x => new UserMapsItem()
                {
                    UserName = x,
                    Maps = this.userMapReader.Query(_ => _.Where(y => y.UserName == x).Select(z => z.Map).ToList())
                }).ToList()
            };
        }
    }
}
