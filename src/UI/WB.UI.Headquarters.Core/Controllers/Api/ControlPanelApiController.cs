using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace WB.UI.Headquarters.Controllers.Api
{
    [Authorize(Roles="Administrator")]
    [Route("api/{controller}/{action}/{id?}")]
    public class ControlPanelApiController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private static readonly Regex ConnectionStringPasswordRegex = new Regex("password=([^;]*);", RegexOptions.IgnoreCase | RegexOptions.Compiled);


        public ControlPanelApiController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public ActionResult<List<KeyValuePair<string, string>>> Configuration()
        {
            List<KeyValuePair<string, string>> keyValuePairs = configuration.AsEnumerable(true).ToList();
            var connectionString = keyValuePairs.FirstOrDefault(x => x.Key == "ConnectionStrings:DefaultConnection");
            keyValuePairs.Remove(connectionString);

            keyValuePairs.Add(
                new KeyValuePair<string, string>(connectionString.Key, RemovePasswordFromConnectionString(connectionString.Value)));

            return keyValuePairs.OrderBy(x => x.Key).ToList();
        }

        private static string RemovePasswordFromConnectionString(string connectionString)
            => ConnectionStringPasswordRegex.Replace(connectionString, "Password=*****;");
    }
}
