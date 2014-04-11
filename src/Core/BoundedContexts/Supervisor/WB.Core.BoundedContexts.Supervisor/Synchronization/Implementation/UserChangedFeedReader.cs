using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Users;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation
{
    internal class UserChangedFeedReader : IUserChangedFeedReader
    {
        private const string AtomXmlNamespace = "http://www.w3.org/2005/Atom";
        private readonly HeadquartersSettings headquartersSettings;
        private readonly HttpMessageHandler messageHandler;

        public UserChangedFeedReader(HeadquartersSettings headquartersSettings, HttpMessageHandler messageHandler)
        {
            if (headquartersSettings == null) throw new ArgumentNullException("headquartersSettings");
            if (messageHandler == null) throw new ArgumentNullException("messageHandler");

            this.headquartersSettings = headquartersSettings;
            this.messageHandler = messageHandler;
        }

        public async Task<List<UserChangedFeedEntry>> ReadAfterAsync(UserChangedFeedEntry lastStoredFeedEntry)
        {
            using (var client = new HttpClient(this.messageHandler))
            {
                var feedUrl = this.headquartersSettings.UserChangedFeedUrl;

                var responseBody = await ReadFeedPage(feedUrl, client);
                XDocument feedDocument = XDocument.Parse(responseBody);
                IEnumerable<UserChangedFeedEntry> entries = ParseFeed(feedDocument);
                Uri archiveUrl = GetArchiveUrl(feedDocument);

                while(archiveUrl != null)
                {
                    if (lastStoredFeedEntry != null && entries.Any(x => x.EntryId == lastStoredFeedEntry.EntryId))
                    {
                        break;
                    }

                    XDocument archiveFeedDocument = XDocument.Parse(await ReadFeedPage(archiveUrl, client));
                    archiveUrl = GetArchiveUrl(archiveFeedDocument);

                    IEnumerable<UserChangedFeedEntry> archiveEntries = ParseFeed(archiveFeedDocument);

                    entries = archiveEntries.Concat(entries);
                }

                if (lastStoredFeedEntry != null)
                {
                    return entries.Where(x => x.Timestamp >= lastStoredFeedEntry.Timestamp && x.EntryId != lastStoredFeedEntry.EntryId).ToList();
                }

                return entries.ToList();
            }
        }

        private static async Task<string> ReadFeedPage(Uri feedUrl, HttpClient client)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, feedUrl);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/atom+xml"));

            HttpResponseMessage response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApplicationException(string.Format("Failed to read users feed. response code: {1}, response content: {2}",
                    response.StatusCode, response.Content));
            }

            string responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }

        private static Uri GetArchiveUrl(XDocument feedDocument)
        {
            var linkElement = feedDocument.Descendants(XName.Get("link", AtomXmlNamespace))
                                        .FirstOrDefault(link => (string)link.Attribute("rel") == "prev-archive");
            return linkElement != null ? new Uri(linkElement.Attribute("href").Value) : null;
        }

        private static IEnumerable<UserChangedFeedEntry> ParseFeed(XDocument feedDocument)
        {
            return from f in feedDocument.Descendants(XName.Get("entry", AtomXmlNamespace))
                let content = JObject.Parse(f.Element(XName.Get("content", AtomXmlNamespace)).Value)
                select new UserChangedFeedEntry(content.Value<string>("SupervisorId"), content.Value<string>("EntryId"))
                {
                    ChangedUserId = content.Value<string>("ChangedUserId"),
                    Timestamp = content.Value<DateTime>("Timestamp")
                };
        }
    }

}