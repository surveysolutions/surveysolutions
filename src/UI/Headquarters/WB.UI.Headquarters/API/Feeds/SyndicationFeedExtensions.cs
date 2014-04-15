using System;
using System.ServiceModel.Syndication;

namespace WB.UI.Headquarters.API.Feeds
{
    public static class SyndicationFeedExtensions
    {
        public static void AppendPrevLink(this SyndicationFeed syndicationFeed, Uri prevPageUri)
        {
            syndicationFeed.Links.Add(new SyndicationLink(prevPageUri, "prev-archive", null, "application/atom+xml", 0));
        }
    }
}