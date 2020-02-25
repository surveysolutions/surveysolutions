using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using Main.Core.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.BrokenInterviewPackages;
using WB.Core.BoundedContexts.Headquarters.Views.TabletInformation;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Enumerator.Native.WebInterview;
using WB.Infrastructure.Native.Monitoring;
using WB.UI.Headquarters.Controllers.Services;
using WB.UI.Headquarters.Models.Api;
using WB.UI.Headquarters.Models.ComponentModels;
using WB.UI.Headquarters.Services;

namespace WB.UI.Headquarters.Controllers.Api
{
    [Authorize(Roles = "Administrator")]
    [Route("api/{controller}/{action}/{id?}")]
    public class ControlPanelApiController : ControllerBase
    {
        private const int DEFAULTPAGESIZE = 12;
        private const string DEFAULTEMPTYQUERY = "";

        private readonly IConfiguration configuration;
        private static readonly Regex ConnectionStringPasswordRegex = new Regex("password=([^;]*);", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private readonly ITabletInformationService tabletInformationService;
        private readonly IClientApkProvider clientApkProvider;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IAndroidPackageReader androidPackageReader;
        private readonly IBrokenInterviewPackagesViewFactory brokenInterviewPackagesViewFactory;
        private readonly IJsonAllTypesSerializer serializer;
        private readonly IInterviewBrokenPackagesService brokenPackagesService;
        private readonly HealthCheckService healthCheckService;

        public ControlPanelApiController(IConfiguration configuration,
            ITabletInformationService tabletInformationService,
            IClientApkProvider clientApkProvider,
            IFileSystemAccessor fileSystemAccessor,
            IAndroidPackageReader androidPackageReader,
            IBrokenInterviewPackagesViewFactory brokenInterviewPackagesViewFactory,
            IJsonAllTypesSerializer serializer,
            IInterviewBrokenPackagesService brokenPackagesService, HealthCheckService healthCheckService)
        {
            this.configuration = configuration;
            this.tabletInformationService = tabletInformationService;
            this.clientApkProvider = clientApkProvider;
            this.fileSystemAccessor = fileSystemAccessor;
            this.androidPackageReader = androidPackageReader;
            this.brokenInterviewPackagesViewFactory = brokenInterviewPackagesViewFactory;
            this.serializer = serializer;
            this.brokenPackagesService = brokenPackagesService;
            this.healthCheckService = healthCheckService;
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
        
        public ActionResult<List<ApkInfo>> AppUpdates()
        {
            var folder = clientApkProvider.ApkClientsFolder();
            var appFiles = Directory.EnumerateFiles(folder);

            var enumerable = appFiles
                .Select(app => new FileInfo(app))
                .OrderBy(fi => fi.Name)
                .Select(fi =>
                {
                    var result = new ApkInfo
                    {
                        FileName = fi.Name
                    };

                    if (fi.Name.EndsWith(".apk", StringComparison.OrdinalIgnoreCase))
                    {
                        var build = this.androidPackageReader.Read(fi.FullName)?.BuildNumber;
                        if(build != null) 
                            result.Build = build;
                        var hash = Convert.ToBase64String(this.fileSystemAccessor.ReadHash(fi.FullName));
                        result.Hash = hash;
                    }

                    result.FileSizeInBytes = fi.Length;
                    result.LastWriteTimeUtc = fi.LastWriteTimeUtc;

                    return result;
                });
            
            return enumerable.ToList();
        }

        public class ApkInfo
        {
            public string FileName { get; set; }
            
            public int? Build { get; set; }
            
            public string Hash { get; set; }
            
            public long FileSizeInBytes { get; set; }
            
            public DateTime LastWriteTimeUtc { get; set; }
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

        [HttpGet]
        [ApiNoCache]
        public ComboboxModel ExceptionTypes(string query = DEFAULTEMPTYQUERY, int pageSize = DEFAULTPAGESIZE)
        {
            var exceptionTypes = this.brokenInterviewPackagesViewFactory.GetExceptionTypes(pageSize: pageSize, searchBy: query);

            return new ComboboxModel(exceptionTypes.ExceptionTypes.Select(x => new ComboboxOptionModel(x, x)).ToArray(),
                (int)exceptionTypes.TotalCountByQuery);
        }

        [HttpGet]
        [ApiNoCache]
        public ComboboxModel ExpectedExceptionTypes(string query = DEFAULTEMPTYQUERY, int pageSize = DEFAULTPAGESIZE)
        {
            var exceptionTypes = Enum.GetValues(typeof(InterviewDomainExceptionType))
                .OfType<InterviewDomainExceptionType>()
                .Select(x => new ComboboxOptionModel($"{(int) x}", x.Humanize()))
                .ToArray();

            return new ComboboxModel(exceptionTypes, exceptionTypes.Length);
        }

        [HttpGet]
        public ActionResult<DataTableResponse<BrokenInterviewPackageView>> InterviewPackages(DataTableInterviewPackageFilter request)
        {
            var filteredItems = this.brokenInterviewPackagesViewFactory.GetFilteredItems(new InterviewPackageFilter
            {
                QuestionnaireIdentity = request.QuestionnaireIdentity,
                ResponsibleId = request.ResponsibleId,
                ExceptionType = request.ExceptionType,
                FromProcessingDateTime = request.FromProcessingDateTime,
                InterviewKey = request.Search?.Value,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                ReturnOnlyUnknownExceptionType = request.ReturnOnlyUnknownExceptionType,
                SortOrder = request.GetSortOrderRequestItems(),
                ToProcessingDateTime = request.ToProcessingDateTime
            });

            return this.Ok(new DataTableResponse<BrokenInterviewPackageView>
            {
                Data = filteredItems.Items,
                RecordsFiltered = filteredItems.Items.Count(),
                RecordsTotal = (int) filteredItems.TotalCount,
                Draw = request.Draw + 1
            });
        }

        public class DataTableInterviewPackageFilter : DataTableRequest
        {
            public Guid? ResponsibleId { get; set; }
            public string QuestionnaireIdentity { get; set; }
            public DateTime? FromProcessingDateTime { get; set; }
            public DateTime? ToProcessingDateTime { get; set; }
            public string ExceptionType { get; set; }
            public bool ReturnOnlyUnknownExceptionType { get; set; }

        }

        [HttpGet]
        [ApiNoCache]
        public IActionResult DownloadSyncPackage(int id, string format)
        {
            var interviewPackage = this.brokenInterviewPackagesViewFactory.GetPackage(id);
            if ("json".Equals(format, StringComparison.OrdinalIgnoreCase))
            {
                var events = this.serializer.Deserialize<AggregateRootEvent[]>(interviewPackage.Events);
                return new JsonResult(events, new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    TypeNameHandling = TypeNameHandling.Objects
                });
            }
            return Content(interviewPackage.Events);
        }

        [HttpPost]
        public IActionResult ReprocessSelectedBrokenPackages([FromBody]ReprocessSelectedBrokenPackagesRequestView request)
        {
            this.brokenPackagesService.ReprocessSelectedBrokenPackages(request.PackageIds);
            return this.Ok();
        }

        [HttpPost]
        public IActionResult MarkReasonAsKnown([FromBody]MarkKnownReasonRequest request)
        {
            this.brokenPackagesService.PutReason(request.PackageIds, request.ErrorType);
            return this.Ok();
        }
        
        [HttpGet]
        public async Task<HealthReport> GetHealthResult(CancellationToken token)
        {
            var report = await healthCheckService.CheckHealthAsync(c => !c.Tags.Contains("ready"), token);
            return report;
        }

        [HttpGet]
        public async Task<List<MetricState>> GetMetricsState(CancellationToken token)
        {
            await MetricsRegistry.Update();
            var result = new List<MetricState>();

            result.Add(new MetricState("Events count", BrokenPackagesStatsCollector.DatabaseTableRowsCount.Labels("events").Value.ToString()));
            result.Add(new MetricState("Events size", BrokenPackagesStatsCollector.DatabaseTableSize.Labels("events").Value.Bytes().Humanize("0.00")));
            result.Add(new MetricState("Working Memory usage", Process.GetCurrentProcess().WorkingSet64.Bytes().Humanize("0.00")));

            var open = CommonMetrics.WebInterviewConnection.GetSummForLabels("open", "*");
            var closed = CommonMetrics.WebInterviewConnection.GetSummForLabels("closed", "*");
            result.Add(new MetricState("Web interview connections", (open - closed).ToString(CultureInfo.InvariantCulture)));
            return result;
        }

        public class MetricState
        {
            public MetricState(string name, string value)
            {
                Name = name;
                Value = value;
            }
            public string Name { get;  }
            public string Value { get; }
        }

        public class ReprocessSelectedBrokenPackagesRequestView
        {
            public int[] PackageIds { get; set; }
        }

        public class MarkKnownReasonRequest
        {
            public int[] PackageIds { get; set; }

            public InterviewDomainExceptionType ErrorType { get; set; }
        }

        public class QuestionnaireView
        {
            public string Title{get; set; }
            public string Identity { get; set; }
        }
    }
}

