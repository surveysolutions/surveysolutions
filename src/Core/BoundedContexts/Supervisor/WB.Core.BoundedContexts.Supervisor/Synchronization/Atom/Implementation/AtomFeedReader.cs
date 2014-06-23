using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Supervisor.Extensions;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization.Atom.Implementation
{
    internal class AtomFeedReader : IAtomFeedReader
    {
        private readonly Func<HttpMessageHandler> messageHandler;
        private readonly HeadquartersSettings headquartersSettings;
        private const string AtomXmlNamespace = "http://www.w3.org/2005/Atom";

        public AtomFeedReader(Func<HttpMessageHandler> messageHandler, HeadquartersSettings settings)
        {
            if (messageHandler == null) throw new ArgumentNullException("messageHandler");
            this.messageHandler = messageHandler;
            this.headquartersSettings = settings;
        }

        public async Task<IEnumerable<AtomFeedEntry<T>>> ReadAfterAsync<T>(Uri feedUri, string lastReceivedEntryId)
        {
            using (var client = new HttpClient(this.messageHandler()))
            {
                client.AppendAuthToken(this.headquartersSettings);

                XDocument feedDocument = await ReadFeedPage(feedUri, client).ConfigureAwait(false);
                IEnumerable<AtomFeedEntry<T>> entries = ParseFeed<T>(feedDocument);
                Uri archiveUrl = GetArchiveUrl(feedDocument);

                while (archiveUrl != null)
                {
                    if (!string.IsNullOrEmpty(lastReceivedEntryId) && entries.Any(x => x.Id == lastReceivedEntryId))
                    {
                        break;
                    }

                    XDocument archiveFeedDocument = await ReadFeedPage(archiveUrl, client).ConfigureAwait(false);
                    archiveUrl = GetArchiveUrl(archiveFeedDocument);

                    IEnumerable<AtomFeedEntry<T>> archiveEntries = ParseFeed<T>(archiveFeedDocument);

                    entries = archiveEntries.Concat(entries).ToList();
                }

                if (!string.IsNullOrEmpty(lastReceivedEntryId))
                {
                    return entries.SkipWhile(x => x.Id != lastReceivedEntryId).Where(x => x.Id != lastReceivedEntryId).ToList();
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

        private static IEnumerable<AtomFeedEntry<T>> ParseFeed<T>(XDocument feedDocument)
        {
            return from f in feedDocument.Descendants(XName.Get("entry", AtomXmlNamespace))
                let links = from l in f.Elements(XName.Get("link", AtomXmlNamespace))
                    select new Link
                    {
                        Href = new Uri(l.Attribute("href").Value),
                        Rel = l.Attribute("rel").Value
                    }
                select new AtomFeedEntry<T>
                {
                    Updated = DateTime.Parse(f.Element(XName.Get("updated", AtomXmlNamespace)).Value),
                    Id = f.Element(XName.Get("id", AtomXmlNamespace)).Value,
                    Content = JsonConvert.DeserializeObject<T>(f.Element(XName.Get("content", AtomXmlNamespace)).Value),
                    Links = links.ToList()
                };
        }
    }
}