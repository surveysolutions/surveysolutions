using System.Collections.Generic;
using System.Globalization;
using CAPI.Android.Syncronization.RestUtils;

namespace CAPI.Android.Syncronization.Update
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
            var newVersionExists = webExecutor.ExcecuteRestRequest<bool>(checkPath,
                null,
                new KeyValuePair<string, string>("version", version),
                new KeyValuePair<string, string>("versionCode", versionCode.ToString(CultureInfo.InvariantCulture)),
                new KeyValuePair<string, string>("androidId", androidId));

            return newVersionExists;
        }

    }
}