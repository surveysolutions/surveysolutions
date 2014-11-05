using System;
using System.Collections.Generic;
using System.Threading;
using WB.Core.GenericSubdomains.Rest;

namespace WB.UI.Capi.Syncronization.Push
{
    public class RestPush
    {
        private readonly IRestServiceWrapper webExecutor;
        private const string PostPackagePath = "api/InterviewerSync/PostPackage";
        private const string PostFilePath = "api/InterviewerSync/PostFile?interviewId={0}"; //don't delete param. web.api mapping sucks
        public RestPush(IRestServiceWrapper webExecutor)
        {
            this.webExecutor = webExecutor;
        }

        public void PushChunck(string login, string password, string content, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(content))
                throw new InvalidOperationException("container is empty");

            var result = this.webExecutor.ExecuteRestRequestAsync<bool>(PostPackagePath, ct, content, login, password, null);

            if (!result)
                throw new RestException(Properties.Resource.PushFailed);
        }

        public void PushBinary(string login, string password, byte[] data, string fileName, Guid interviewId, CancellationToken ct)
        {
            if (data == null)
                throw new InvalidOperationException("data is empty");

            var pathToPostFile = string.Format(PostFilePath, interviewId);

            var result = this.webExecutor.ExecuteRestRequestAsync<bool>(pathToPostFile, 
                new []{new KeyValuePair<string, object>("interviewId", interviewId)}, 
                ct,
                data, fileName, login, password, null, 
                new KeyValuePair<string, object>[]{});

            if (!result)
                throw new RestException(Properties.Resource.PushBinaryDataFailed);
        }
    }
}