using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.TabletInformation;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.UI.Headquarters.Models.Api;

namespace WB.UI.Headquarters.Controllers.Api
{
    [Authorize(Roles = "Administrator")]
    [Route("api/{controller}/{action}/{id?}")]
    public class ControlPanelApiController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private static readonly Regex ConnectionStringPasswordRegex = new Regex("password=([^;]*);", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private readonly ITabletInformationService tabletInformationService;
        
        public ControlPanelApiController(IConfiguration configuration,
            ITabletInformationService tabletInformationService)
        {
            this.configuration = configuration;
            this.tabletInformationService = tabletInformationService;
        }


        public ActionResult<DataTableResponse<TabletInformationView>> TabletInfos(DataTableRequest request)
        {
            var items = this.tabletInformationService.GetAllTabletInformationPackages();

            if (!string.IsNullOrEmpty(request.Search?.Value))
                items = items.Where(x => x.UserName != null && x.UserName.StartsWith(request.Search.Value, StringComparison.OrdinalIgnoreCase)).ToList();

            var itemsSlice = items.Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();

            foreach (var tabletInformationView in itemsSlice)
            {
                tabletInformationView.DownloadUrl = Url.Action("Download", new {id=tabletInformationView.PackageName});
            }
            return new DataTableResponse<TabletInformationView>
            {
                Data = itemsSlice,
                Draw = request.Draw + 1,
                RecordsFiltered = items.Count,
                RecordsTotal = items.Count
            };
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

        public IActionResult Download(string id)
        {
            var hostName = this.Request.Host.ToString().Split('.').FirstOrDefault() ?? @"unknownhost";
            var fullPathToContentFile = this.tabletInformationService.GetFullPathToContentFile(id);

            return this.File(System.IO.File.OpenRead(fullPathToContentFile), "application/zip",
                this.tabletInformationService.GetFileName(id, hostName));
        }
    }
}

