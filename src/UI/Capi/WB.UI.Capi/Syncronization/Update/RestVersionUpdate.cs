using System.Collections.Generic;
using System.Globalization;
using WB.UI.Shared.Android.RestUtils;

namespace WB.UI.Capi.Syncronization.Update
{
    public class RestVersionUpdate
    {
        private readonly IRestUrils webExecutor;

        private const string checkPath = "sync/CheckNewVersion";

        public RestVersionUpdate(IRestUrils webExecutor)
        {
            this.webExecutor = webExecutor;
        }

        public bool Execute(string version, int versionCode, string androidId)
        {
            var newVersionExists = this.webExecutor.ExcecuteRestRequest<bool>(checkPath,
                null,null,
                new KeyValuePair<string, string>("version", version),
                new KeyValuePair<string, string>("versionCode", versionCode.ToString(CultureInfo.InvariantCulture)),
                new KeyValuePair<string, string>("androidId", androidId));

            return newVersionExists;
        }

    }
}