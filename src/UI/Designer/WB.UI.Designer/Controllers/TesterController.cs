using System.Globalization;
using System.IO;
using System.Web.Mvc;
using WB.UI.Shared.Web.Membership;

namespace WB.UI.Designer.Controllers
{
    public class TesterController : BaseController
    {
        private readonly string capiTesterFileName = "WBCapiTester.apk";
        private readonly string pathToSearchVersions = "~/App_Data/Capi";

        public TesterController(IMembershipUserService userHelper) : base(userHelper)
        {
        }
        
        [AllowAnonymous]
        public ActionResult Index()
        {
            int maxVersion = this.GetLastVersionNumber();

            if (maxVersion > 0)
            {
                var targetToSearchVersions = Server.MapPath(pathToSearchVersions);
                string path = Path.Combine(targetToSearchVersions, maxVersion.ToString(CultureInfo.InvariantCulture));

                string pathToFile = Path.Combine(path, this.capiTesterFileName);
                if (System.IO.File.Exists(pathToFile))
                    return File(pathToFile, 
                                "application/vnd.android.package-archive", 
                                GetApkFileNameWithVersion(this.capiTesterFileName, maxVersion));
            }
            return null;
        }

        private string GetApkFileNameWithVersion(string fileName, int version)
        {
            return fileName.Replace(".apk", string.Format(".v{0}.apk", version));
        }

        private int GetLastVersionNumber()
        {
            int maxVersion = 0;

            var targetToSearchVersions = Server.MapPath(pathToSearchVersions);
            if (Directory.Exists(targetToSearchVersions))
            {
                var dirInfo = new DirectoryInfo(targetToSearchVersions);
                foreach (DirectoryInfo directoryInfo in dirInfo.GetDirectories())
                {
                    int value;
                    if (int.TryParse(directoryInfo.Name, out value))
                        if (maxVersion < value)
                            maxVersion = value;
                }
            }

            return maxVersion;
        }

    }
}
