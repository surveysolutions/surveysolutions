using System;
using System.Web;
using System.Web.Caching;

namespace WB.UI.WebTester.Services.Implementation
{
    public class MediaStorage : IMediaStorage
    {
        private Cache Cache => HttpRuntime.Cache;

        public void Store(MultimediaFile file, Guid interviewId)
        {
            Cache.Insert(key(interviewId, file.Filename), file, null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(5));
        }

        public MultimediaFile Get(Guid interviewId, string filename)
        {
            return Cache.Get(key(interviewId, filename)) as MultimediaFile;
        }

        string key(Guid interviewId, string filename) => $"{interviewId.ToString()}:{filename}";
    }
}