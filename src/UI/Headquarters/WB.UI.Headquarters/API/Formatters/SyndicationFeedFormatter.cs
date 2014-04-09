using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;

namespace WB.UI.Headquarters.API.Formatters
{
    public class SyndicationFeedFormatter : MediaTypeFormatter
    {
        private readonly string atom = "application/atom+xml";
        private readonly string rss = "application/rss+xml";

        public SyndicationFeedFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue(atom));
            SupportedMediaTypes.Add(new MediaTypeHeaderValue(rss));
        }

        public override bool CanReadType(Type type)
        {
            return false;
        }

        public override bool CanWriteType(Type type)
        {
            if (type == typeof(SyndicationFeed))
                return true;
            return false;
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            return Task.Factory.StartNew(() =>
            {
                if (type == typeof(SyndicationFeed))
                    BuildSyndicationFeed((SyndicationFeed)value, writeStream, content.Headers.ContentType.MediaType);
            });
        }

        private void BuildSyndicationFeed(SyndicationFeed feed, Stream stream, string contenttype)
        {
            if (feed == null) throw new ArgumentNullException("feed");

            using (XmlWriter writer = XmlWriter.Create(stream))
            {
                if (string.Equals(contenttype, atom))
                {
                    var atomformatter = new Atom10FeedFormatter(feed);
                    atomformatter.WriteTo(writer);
                }
                else
                {
                    var rssformatter = new Rss20FeedFormatter(feed);
                    rssformatter.WriteTo(writer);
                }
            }
        }
    }
}