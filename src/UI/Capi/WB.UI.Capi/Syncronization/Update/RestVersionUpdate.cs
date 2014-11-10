using System.Collections.Generic;
using System.Globalization;
using WB.Core.GenericSubdomains.Rest;
using WB.Core.GenericSubdomains.Utils.Rest;

namespace WB.UI.Capi.Syncronization.Update
{
    public class RestVersionUpdate
    {
        private readonly IRestServiceWrapper webExecutor;

        private const string checkPath = "api/InterviewerSync/CheckNewVersion";

        public RestVersionUpdate(IRestServiceWrapper webExecutor)
        {
            this.webExecutor = webExecutor;
        }

        public bool Execute(int versionCode)
        {
            var newVersionExists = this.webExecutor.ExecuteRestRequest<bool>(checkPath,null,null,"GET",
                new KeyValuePair<string, object>("versionCode", versionCode.ToString(CultureInfo.InvariantCulture)));

            return newVersionExists;
        }

    }
}