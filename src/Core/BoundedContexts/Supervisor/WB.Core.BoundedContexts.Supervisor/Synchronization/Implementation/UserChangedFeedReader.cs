using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;

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

        public async Task<List<LocalUserChangedFeedEntry>> ReadAfterAsync(LocalUserChangedFeedEntry lastStoredFeedEntry)
        {
            using (var client = new HttpClient(this.messageHandler))
            {
                var feedUrl = this.headquartersSettings.UserChangedFeedUrl;

                XDocument feedDocument = await ReadFeedPage(feedUrl, client).ConfigureAwait(false);
                IEnumerable<LocalUserChangedFeedEntry> entries = ParseFeed(feedDocument);
                Uri archiveUrl = GetArchiveUrl(feedDocument);

                while(archiveUrl != null)
                {
                    if (lastStoredFeedEntry != null && entries.Any(x => x.EntryId == lastStoredFeedEntry.EntryId))
                    {
                        break;
                    }

                    XDocument archiveFeedDocument = await ReadFeedPage(archiveUrl, client).ConfigureAwait(false);
                    archiveUrl = GetArchiveUrl(archiveFeedDocument);

                    IEnumerable<LocalUserChangedFeedEntry> archiveEntries = ParseFeed(archiveFeedDocument);

                    entries = archiveEntries.Concat(entries).ToList();
                }

                if (lastStoredFeedEntry != null)
                {
                    return entries.Where(x => x.Timestamp >= lastStoredFeedEntry.Timestamp && x.EntryId != lastStoredFeedEntry.EntryId).ToList();
                }

                return entries.ToList();
            }
        }

        private static async Task<XDocument> ReadFeedPage(Uri feedUrl, HttpClient client)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, feedUrl);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/atom+xml"));

            HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApplicationException(string.Format("Failed to read users feed. response code: {1}, response content: {2}",
                    response.StatusCode, response.Content));
            }

            string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            return XDocument.Parse(responseBody);
        }

        private static Uri GetArchiveUrl(XDocument feedDocument)
        {
            var linkElement = feedDocument.Descendants(XName.Get("link", AtomXmlNamespace))
                                        .FirstOrDefault(link => (string)link.Attribute("rel") == "prev-archive");
            return linkElement != null ? new Uri(linkElement.Attribute("href").Value) : null;
        }

        private static IEnumerable<LocalUserChangedFeedEntry> ParseFeed(XDocument feedDocument)
        {
            return from f in feedDocument.Descendants(XName.Get("entry", AtomXmlNamespace))
                let content = JObject.Parse(f.Element(XName.Get("content", AtomXmlNamespace)).Value)
                let links = f.Descendants(XName.Get("link", AtomXmlNamespace))
                select new LocalUserChangedFeedEntry(content.Value<string>("SupervisorId"), content.Value<string>("EntryId"))
                {
                    ChangedUserId = content.Value<string>("ChangedUserId"),
                    Timestamp = content.Value<DateTime>("Timestamp"),
                    UserDetailsUri = new Uri(links.First(l => l.Attribute("rel").Value == "enclosure").Attribute("href").Value)
                };
        }
    }

}