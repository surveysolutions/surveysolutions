using System.ServiceModel.Syndication;
using System.Web.Http;

namespace WB.UI.Headquarters.API.Feeds
{
    public class UsersFeedController : ApiController
    {

        //private SyndicationItem BuildSyndicationItem(Url u)
        //{
        //    var item = new SyndicationItem()
        //    {
        //        Title = new TextSyndicationContent(u.Title),
        //        BaseUri = new Uri(u.Address),
        //        LastUpdatedTime = u.CreatedAt,
        //        Content = new TextSyndicationContent(u.Description)
        //    };
        //    item.Authors.Add(new SyndicationPerson() { Name = u.CreatedBy });

        //    return item;
        //}

        [Route("api/feeds/users/v1")]
        [HttpGet]
        public SyndicationFeed Index()
        {
            return new SyndicationFeed("Title", "Description", null);
        }
    }
}