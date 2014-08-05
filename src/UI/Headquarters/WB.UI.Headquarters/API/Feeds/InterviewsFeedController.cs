using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel.Syndication;
using System.Web.Http;
using Humanizer;
using Newtonsoft.Json;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Interview;
using WB.UI.Headquarters.API.Attributes;
using WB.UI.Headquarters.API.Filters;

namespace WB.UI.Headquarters.API.Feeds
{
    [TokenValidationAuthorizationAttribute]
    [HeadquarterFeatureOnly]
    public class InterviewsFeedController : ApiController
    {
        private const int PageSize = 100;
        private const string FeedTitle = "Interviews feed";
        private const string FeedId = "4a07ced802d44447aa350519937502f3";
        private const string ArchiveRouteName = "api.interviewsFeedArchive";
        private readonly IQueryableReadSideRepositoryReader<InterviewFeedEntry> feedReader;

        public InterviewsFeedController(IQueryableReadSideRepositoryReader<InterviewFeedEntry> feedReader)
        {
            if (feedReader == null) throw new ArgumentNullException("feedReader");
            this.feedReader = feedReader;
        }

        [Route("api/feeds/interviews/v1")]
        [HttpGet]
        public SyndicationFeed Get()
        {
            var interviewFeedEntries = new List<InterviewFeedEntry>();
            int totalFeedEntriesCount = this.feedReader.Count();

            if (totalFeedEntriesCount % PageSize > 0)
            {
                interviewFeedEntries = feedReader.Query(_ => _.SkipFullPages(PageSize, totalFeedEntriesCount).OrderBy(x => x.Timestamp).ToList());
            }

            var feed = this.GetFeed(interviewFeedEntries);

            if (totalFeedEntriesCount >= PageSize)
            {
                var prevPage = totalFeedEntriesCount / PageSize;
                this.AppendPrevLink(prevPage, feed);
            }

            return feed;

        }

        [Route("api/feeds/interviews/v1/archive/{page:int:min(1)=1}", Name = ArchiveRouteName)]
        [HttpGet]
        public HttpResponseMessage Archive(int page)
        {
            var changedFeedEntries = feedReader.Query(_ => _.GetPage(page, PageSize).OrderBy(x => x.Timestamp).ToList());
            if (changedFeedEntries.Count == 0)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Page is empty or not created yet");
            }

            SyndicationFeed syndicationFeed = this.GetFeed(changedFeedEntries);

            if (page > 1)
            {
                this.AppendPrevLink(page - 1, syndicationFeed);
            }

            var response = Request.CreateResponse(syndicationFeed);
            response.Content.Headers.LastModified = changedFeedEntries.Last().Timestamp;
            response.Headers.CacheControl = Constants.DefaultArchiveCache();

            return response;
        }

        private void AppendPrevLink(int pageNumber, SyndicationFeed syndicationFeed)
        {
            string prevPageUrl = this.Url.Route(ArchiveRouteName, new { page = pageNumber });
            var prevPageUri = new Uri(this.Request.RequestUri, prevPageUrl);
            syndicationFeed.AppendPrevLink(prevPageUri);
        }

        private SyndicationFeed GetFeed(List<InterviewFeedEntry> interviewFeedEntries)
        {
            var feed = new SyndicationFeed
            {
                Title = new TextSyndicationContent(FeedTitle),
                LastUpdatedTime = interviewFeedEntries.Count > 0 ? interviewFeedEntries.Last().Timestamp : DateTime.Now,
                Items = this.GenerateFeedItems(interviewFeedEntries),
                Id = FeedId
            };
            return feed;
        }

        private IEnumerable<SyndicationItem> GenerateFeedItems(IEnumerable<InterviewFeedEntry> feedEntries)
        {
            foreach (InterviewFeedEntry entry in feedEntries)
            {
                var item = new SyndicationItem
                {
                    Title = new TextSyndicationContent(entry.EntryType.Humanize()),
                    LastUpdatedTime = entry.Timestamp,
                    Id = entry.EntryId
                };

                string interivewDetailsUrl = this.Url.Route("api.interviewDetails", new { id = entry.InterviewId }); 
                item.Links.Add(new SyndicationLink(new Uri(this.Request.RequestUri, interivewDetailsUrl), "enclosure", "Related interview", null, 0));

                item.Content = new TextSyndicationContent(
                    JsonConvert.SerializeObject(entry, Formatting.Indented,
                        new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            TypeNameHandling = TypeNameHandling.None
                        }));

                yield return item;
            }
        }
    }
}