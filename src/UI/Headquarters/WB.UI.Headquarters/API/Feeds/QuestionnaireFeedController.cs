using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Syndication;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Questionnaire;
using WB.UI.Headquarters.API.Attributes;

namespace WB.UI.Headquarters.API.Feeds
{
    [TokenValidationAuthorization]
    [HeadquarterFeatureOnly]
    public class QuestionnaireFeedController : ApiController
    {
        private readonly IQueryableReadSideRepositoryReader<QuestionnaireFeedEntry> questionnaireChangedReader;
        private const int PageSize = 10;
        private const string FeedTitle = "Questionnaire changed feed";
        private const string FeedId = "b8345e5b2da447fea95a981ea2f7d3a0";

        public QuestionnaireFeedController(IQueryableReadSideRepositoryReader<QuestionnaireFeedEntry> questionnaireChangedReader)
        {
            this.questionnaireChangedReader = questionnaireChangedReader;
        }

        [Route("api/feeds/questionnaires/v1")]
        [HttpGet]
        public SyndicationFeed Index()
        {
            var questionnaireChangedFeedEntries = new List<QuestionnaireFeedEntry>();
            int totalFeedEntriesCount = this.questionnaireChangedReader.Count();

            if (totalFeedEntriesCount%PageSize > 0)
            {
                questionnaireChangedFeedEntries = this.questionnaireChangedReader.Query(_ => _.SkipFullPages(PageSize, totalFeedEntriesCount).OrderBy(x => x.Timestamp).ToList());
            }

            var feed = this.GetFeed(questionnaireChangedFeedEntries);

            if (totalFeedEntriesCount >= PageSize)
            {
                var prevPage = totalFeedEntriesCount / PageSize;
                this.AppendPrevLink(prevPage, feed);
            }

            return feed;
        }

        [Route("api/feeds/questionnaires/v1/archive/{page:int:min(1)=1}", Name = "api.questionnairesFeedArchive")]
        [HttpGet]
        public HttpResponseMessage Archive(int page)
        {
            var changedFeedEntries = this.questionnaireChangedReader.Query(_ => _.GetPage(page, PageSize).OrderBy(x => x.Timestamp).ToList());
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
            string prevPageUrl = this.Url.Route("api.questionnairesFeedArchive", new { page = pageNumber });
            var prevPageUri = new Uri(this.Request.RequestUri, prevPageUrl);
            syndicationFeed.AppendPrevLink(prevPageUri);
        }

        private SyndicationFeed GetFeed(List<QuestionnaireFeedEntry> questionnaireChangedFeedEntries)
        {
            var feed = new SyndicationFeed
            {
                Title = new TextSyndicationContent(FeedTitle),
                LastUpdatedTime = questionnaireChangedFeedEntries.Count > 0 ? questionnaireChangedFeedEntries.Last().Timestamp : DateTime.Now,
                Items = this.GenerateFeedItems(questionnaireChangedFeedEntries),
                Id = FeedId
            };
            return feed;
        }

        private IEnumerable<SyndicationItem> GenerateFeedItems(IEnumerable<QuestionnaireFeedEntry> questionnaireChangedFeedEntries)
        {
            foreach (QuestionnaireFeedEntry entry in questionnaireChangedFeedEntries)
            {
                var item = new SyndicationItem
                {
                    Title = new TextSyndicationContent("Questionnaire changed"),
                    LastUpdatedTime = entry.Timestamp,
                    Id = entry.EntryId
                };

                string detailsUrl = this.Url.Route("api.questionnaireDetails",
                    new { id = entry.QuestionnaireId, version = entry.QuestionnaireVersion });

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