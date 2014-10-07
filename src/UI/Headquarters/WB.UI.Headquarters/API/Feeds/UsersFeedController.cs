using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel.Syndication;
using System.Web.Http;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Headquarters.Users.Denormalizers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Users;
using WB.UI.Headquarters.API.Attributes;
using WB.UI.Headquarters.API.Filters;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.API.Feeds
{
    [TokenValidationAuthorization]
    [HeadquarterFeatureOnly]
    public class UsersFeedController : ApiController
    {
        private readonly IQueryableReadSideRepositoryReader<UserChangedFeedEntry> userChangedReader;
        private const int PageSize = 10;
        private const string FeedTitle = "Users changed feed";
        private const string FeedId = "e0288c6203934e3ebf98f624bac16265";

        public UsersFeedController(IQueryableReadSideRepositoryReader<UserChangedFeedEntry> userChangedReader)
        {
            this.userChangedReader = userChangedReader;
        }

        [Route("api/feeds/users/v1")]
        [HttpGet]
        public SyndicationFeed Index()
        {
            var userChangedFeedEntries = new List<UserChangedFeedEntry>();
            int totalFeedEntriesCount = this.userChangedReader.Count();

            if (totalFeedEntriesCount%PageSize > 0)
            {
                userChangedFeedEntries = userChangedReader.Query(_ => _.SkipFullPages(PageSize, totalFeedEntriesCount).OrderBy(x => x.Timestamp).ToList());
            }

            var feed = this.GetFeed(userChangedFeedEntries);

            if (totalFeedEntriesCount >= PageSize)
            {
                var prevPage = totalFeedEntriesCount / PageSize;
                this.AppendPrevLink(prevPage, feed);
            }

            return feed;
        }

        [Route("api/feeds/users/v1/archive/{page:int:min(1)=1}", Name = "api.usersFeedArchive")]
        [HttpGet]
        public HttpResponseMessage Archive(int page)
        {
            var changedFeedEntries = userChangedReader.Query(_ => _.GetPage(page, PageSize).OrderBy(x => x.Timestamp).ToList());
            SyndicationFeed syndicationFeed = this.GetFeed(changedFeedEntries);

            if (page > 1)
            {
                this.AppendPrevLink(page - 1, syndicationFeed);
            }

            var response = new HttpResponseMessage();
            response.Content = new ObjectContent(typeof(SyndicationFeed), syndicationFeed, new Formatters.SyndicationFeedFormatter());
            response.Content.Headers.LastModified = changedFeedEntries.Last().Timestamp;

            response.Headers.CacheControl = Constants.DefaultArchiveCache();

            return response;
        }

        private void AppendPrevLink(int pageNumber, SyndicationFeed syndicationFeed)
        {
            string prevPageUrl = this.Url.Route("api.usersFeedArchive", new { page = pageNumber });
            var prevPageUri = new Uri(this.Request.RequestUri, prevPageUrl);
            syndicationFeed.AppendPrevLink(prevPageUri);
        }

        private SyndicationFeed GetFeed(List<UserChangedFeedEntry> userChangedFeedEntries)
        {
            var feed = new SyndicationFeed
            {
                Title = new TextSyndicationContent(FeedTitle),
                LastUpdatedTime = userChangedFeedEntries.Count > 0 ? userChangedFeedEntries.Last().Timestamp : DateTime.Now,
                Items = this.GenerateFeedItems(userChangedFeedEntries),
                Id = FeedId
            };
            return feed;
        }

        private IEnumerable<SyndicationItem> GenerateFeedItems(IEnumerable<UserChangedFeedEntry> userChangedFeedEntries)
        {
            foreach (UserChangedFeedEntry entry in userChangedFeedEntries)
            {
                var item = new SyndicationItem
                {
                    Title = new TextSyndicationContent("User changed"),
                    LastUpdatedTime = entry.Timestamp,
                    Id = entry.EntryId
                };

                string detailsUrl = this.Url.Route("api.userDetails", new { id = entry.ChangedUserId });
                item.Links.Add(new SyndicationLink(new Uri(this.Request.RequestUri, detailsUrl), "enclosure", null, null, 0));

                item.Content = new TextSyndicationContent(
                    JsonConvert.SerializeObject(entry, Formatting.Indented, 
                        new JsonSerializerSettings {
                            NullValueHandling = NullValueHandling.Ignore,
                            TypeNameHandling = TypeNameHandling.None
                        }));

                yield return item;
            }
        }
    }
}