using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.Users.Denormalizers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Utils.Serialization;
using WB.UI.Headquarters.Models;

namespace WB.UI.Headquarters.API.Feeds
{
    public class UsersFeedController : ApiController
    {
        private readonly IQueryableReadSideRepositoryReader<UserChangedFeedEntry> userChangedReader;
        private readonly IJsonUtils jsonUtils;

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

        public UsersFeedController(IQueryableReadSideRepositoryReader<UserChangedFeedEntry> userChangedReader,
            IJsonUtils jsonUtils)
        {
            this.userChangedReader = userChangedReader;
            this.jsonUtils = jsonUtils;
        }

        [Route("api/feeds/users/v1")]
        [HttpGet]
        public SyndicationFeed Index()
        {
            List<UserChangedFeedEntry> userChangedFeedEntries = userChangedReader.Query(_ => _.OrderBy(x => x.Timestamp).ToList());

            var feed = new SyndicationFeed();
            feed.Title = new TextSyndicationContent("Users changed feed");

            var items = new List<SyndicationItem>();
            foreach (var entry in userChangedFeedEntries)
            {
                var item = new SyndicationItem
                {
                    Title = new TextSyndicationContent("User changed"),
                    LastUpdatedTime = entry.Timestamp,
                    Id = entry.EntryId
                };

                string detailsUrl = Url.Route("api.userDetails", new { id = entry.ChangedUserId });
                item.Links.Add(new SyndicationLink(new Uri(Request.RequestUri, detailsUrl), "enclosure", null, null, 0));

                item.Content = new TextSyndicationContent(jsonUtils.GetItemAsContent(entry));
                items.Add(item);
            }
            feed.Items = items;

            return feed;
        }
    }
}