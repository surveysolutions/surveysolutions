using System;
using System.Web.Http.Filters;
using Ninject;
using WB.Core.BoundedContexts.Designer.UsageStats.QuestionnaireImport;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;

namespace WB.UI.Designer.Api.Attributes
{
    public class LogImportActionAttribute : ActionFilterAttribute
    {
        [Inject]
        public IPlainStorageAccessor<QuestionnaireImportedEntry> importLogStorage { get; set; }

        [Inject]
        public ILoggerProvider Logger { get; set; }

        public override void OnActionExecuted(HttpActionExecutedContext context)
        {
            if (context.Response.IsSuccessStatusCode)
            {
                try
                {
                    QuestionnaireImportedEntry logEntry = new QuestionnaireImportedEntry
                    {
                        ImportDateUtc = DateTime.UtcNow
                    };

                    var request = context.ActionContext.ActionArguments["request"] as DownloadQuestionnaireRequest;
                    logEntry.QuestionnaireId = request.QuestionnaireId;
                    logEntry.SupportedByHqVersion = new[] {request.SupportedVersion.Major, request.SupportedVersion.Minor, request.SupportedVersion.Patch};

                    importLogStorage.Store(logEntry, logEntry.ImportDateUtc);
                }
                catch (Exception e)
                {
                    Logger.GetFor<LogImportActionAttribute>().Warn(e.Message, e);
                }
            }
        }
    }
}