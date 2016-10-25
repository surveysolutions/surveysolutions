using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;
using System.Web.Http.Routing;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.UI.Headquarters.Resources;
using WB.UI.Headquarters.Utils;

namespace WB.UI.Headquarters.Code
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class WriteToSyncLogAttribute : ActionFilterAttribute
    {
        private const string UnknownStringArgumentValue = "unknown";

        private readonly SynchronizationLogType logAction;

        private IPlainStorageAccessor<SynchronizationLogItem> synchronizationLogItemPlainStorageAccessor
            => ServiceLocator.Current.GetInstance<IPlainStorageAccessor<SynchronizationLogItem>>();

        private IGlobalInfoProvider globalInfoProvider => ServiceLocator.Current.GetInstance<IGlobalInfoProvider>();

        private IUserWebViewFactory userInfoViewFactory => ServiceLocator.Current.GetInstance<IUserWebViewFactory>();

        private IQuestionnaireBrowseViewFactory questionnaireBrowseItemFactory
            => ServiceLocator.Current.GetInstance<IQuestionnaireBrowseViewFactory>();

        private IUserViewFactory userViewFactory => ServiceLocator.Current.GetInstance<IUserViewFactory>();

        private ILogger logger => ServiceLocator.Current.GetInstance<ILoggerProvider>().GetFor<WriteToSyncLogAttribute>();

        public WriteToSyncLogAttribute(SynchronizationLogType logAction)
        {
            this.logAction = logAction;
        }

        public override void OnActionExecuted(HttpActionExecutedContext context)
        {
            base.OnActionExecuted(context);

            try
            {
                var logItem = new SynchronizationLogItem
                {
                    DeviceId = this.GetInterviewerDeviceId(),
                    InterviewerId = this.globalInfoProvider.GetCurrentUser().Id,
                    InterviewerName = this.globalInfoProvider.GetCurrentUser().Name,
                    LogDate = DateTime.UtcNow,
                    Type = this.logAction
                };

                switch (this.logAction)
                {
                    case SynchronizationLogType.CanSynchronize:
                        logItem.DeviceId = context.GetActionArgumentOrDefault<string>("id", string.Empty);
                        if (context.Response.IsSuccessStatusCode) 
                            logItem.Log = SyncLogMessages.CanSynchronize;
                        else if (context.Response.StatusCode == HttpStatusCode.UpgradeRequired)
                            logItem.Log =  SyncLogMessages.DeviceUpdateRequired.FormatString(context.GetActionArgumentOrDefault<int>("version", -1));
                        else if (context.Response.StatusCode == HttpStatusCode.Forbidden)
                            logItem.Log = SyncLogMessages.DeviceRelinkRequired;
                        break;
                    case SynchronizationLogType.HasInterviewerDevice:
                        logItem.Log = string.IsNullOrEmpty(logItem.DeviceId) ? SyncLogMessages.DeviceCanBeAssignedToInterviewer : SyncLogMessages.InterviewerHasDevice;
                        break;
                    case SynchronizationLogType.LinkToDevice:
                        logItem.DeviceId = context.GetActionArgumentOrDefault<string>("id", string.Empty);
                        logItem.Log = SyncLogMessages.LinkToDevice;
                        break;
                    case SynchronizationLogType.GetInterviewer:
                        logItem.Log = this.GetInterviewerLogMessage(context);
                        break;
                    case SynchronizationLogType.GetCensusQuestionnaires:
                        logItem.Log = this.GetQuestionnairesLogMessage(context);
                        break;
                    case SynchronizationLogType.GetQuestionnaire:
                        logItem.Log = this.GetQuestionnaireLogMessage(SyncLogMessages.GetQuestionnaire, context);
                        break;
                    case SynchronizationLogType.QuestionnaireProcessed:
                        logItem.Log = this.GetQuestionnaireLogMessage(SyncLogMessages.QuestionnaireProcessed, context);
                        break;
                    case SynchronizationLogType.GetQuestionnaireAssembly:
                        logItem.Log = this.GetQuestionnaireLogMessage(SyncLogMessages.GetQuestionnaireAssembly, context);
                        break;
                    case SynchronizationLogType.QuestionnaireAssemblyProcessed:
                        logItem.Log = this.GetQuestionnaireLogMessage(SyncLogMessages.QuestionnaireAssemblyProcessed, context);
                        break;
                    case SynchronizationLogType.GetInterviewPackage:
                        logItem.Log = SyncLogMessages.GetInterviewPackage.FormatString(context.GetActionArgumentOrDefault<string>("id", string.Empty));
                        break;
                    case SynchronizationLogType.InterviewPackageProcessed:
                        logItem.Log = SyncLogMessages.InterviewPackageProcessed.FormatString(context.GetActionArgumentOrDefault<string>("id", string.Empty));
                        break;
                    case SynchronizationLogType.GetInterviews:
                        logItem.Log = this.GetInterviewsLogMessage(context);
                        break;
                    case SynchronizationLogType.GetInterview:
                        logItem.Log = SyncLogMessages.GetInterview.FormatString(context.GetActionArgumentOrDefault<Guid>("id", Guid.Empty));
                        break;
                    case SynchronizationLogType.InterviewProcessed:
                        logItem.Log = SyncLogMessages.InterviewProcessed.FormatString(context.GetActionArgumentOrDefault<Guid>("id", Guid.Empty));
                        break;
                    case SynchronizationLogType.GetQuestionnaireAttachments:
                        logItem.Log = this.GetQuestionnaireLogMessage(SyncLogMessages.GetQuestionnaireAttachments, context);
                        break;
                    case SynchronizationLogType.GetAttachmentContent:
                        logItem.Log = SyncLogMessages.GetAttachmentContent.FormatString(context.GetActionArgumentOrDefault<string>("id", string.Empty));
                        break;
                    case SynchronizationLogType.PostInterview:
                        var interviewId = context.GetActionArgumentOrDefault<InterviewPackageApiView>("package", null)?.InterviewId;
                        logItem.Log = SyncLogMessages.PostPackage.FormatString(interviewId.HasValue ? GetInterviewLink(context, interviewId.Value) : UnknownStringArgumentValue, interviewId);
                        break;
                    case SynchronizationLogType.PostPackage:
                        var packageId = context.GetActionArgumentOrDefault<Guid>("id", Guid.Empty);
                        logItem.Log = SyncLogMessages.PostPackage.FormatString(GetInterviewLink(context, packageId), packageId);
                        break;

                    default:
                        throw new ArgumentException("logAction");
                }
                this.synchronizationLogItemPlainStorageAccessor.Store(logItem, Guid.NewGuid());
            }
            catch (Exception exception)
            {
                this.logger.Error("Error updating sync log.", exception);
            }
        }

        private string GetInterviewsLogMessage(HttpActionExecutedContext context)
        {
            var interviewsApiView = this.GetResponseObject<List<InterviewApiView>>(context);

            var messagesByInterviews = interviewsApiView.Select(x => GetInterviewLink(context, x.Id)).ToList();

            var readability = !messagesByInterviews.Any()
                ? SyncLogMessages.NoNewInterviewPackagesToDownload
                : string.Join("<br />", messagesByInterviews);
            return SyncLogMessages.GetInterviews.FormatString(readability);
        }

        private static string GetInterviewLink(HttpActionExecutedContext context, Guid interviewId)
        {
            return new UrlHelper(context.Request).Link("Default",
                new { controller = "Interview", action = "Details", id = interviewId });
        }

        private string GetMessageByInterviewPackageType(HttpActionExecutedContext context, SynchronizationChunkMeta synchronizationChunkMeta)
        {
            if (synchronizationChunkMeta.ItemType == SyncItemType.Interview)
            {
                return SyncLogMessages.UpdateInterviewPackage.FormatString(synchronizationChunkMeta.InterviewId,
                    synchronizationChunkMeta.Id, synchronizationChunkMeta.SortIndex,
                    new UrlHelper(context.Request).Link("Default",
                        new {controller = "Interview", action = "Details", id = synchronizationChunkMeta.InterviewId}));
            }

            if (synchronizationChunkMeta.ItemType == SyncItemType.DeleteInterview)
            {
                return SyncLogMessages.DeleteInterviewPackage.FormatString(synchronizationChunkMeta.InterviewId,
                    synchronizationChunkMeta.Id, synchronizationChunkMeta.SortIndex,
                    new UrlHelper(context.Request).Link("Default",
                        new { controller = "Interview", action = "Details", id = synchronizationChunkMeta.InterviewId }));
            }

            return "Unknown interview package type";
        }

        private string GetInterviewerLogMessage(HttpActionExecutedContext context)
        {
            var interviewerApiView = this.GetResponseObject<InterviewerApiView>(context);

            var supervisorInfo = this.userViewFactory.Load(new UserViewInputModel(interviewerApiView.SupervisorId));
            return SyncLogMessages.GetInterviewer.FormatString(supervisorInfo.UserName, supervisorInfo.PublicKey.FormatGuid());
        }

        private string GetQuestionnairesLogMessage(HttpActionExecutedContext context)
        {
            List<QuestionnaireIdentity> censusQuestionnaireIdentities =
                this.GetResponseObject<List<QuestionnaireIdentity>>(context);

            var censusQuestionnaires = censusQuestionnaireIdentities.Select(x => this.questionnaireBrowseItemFactory.GetById(new QuestionnaireIdentity(x.QuestionnaireId, x.Version)));

            var messagesByCensusQuestionnaires = censusQuestionnaires.Select(
                censusQuestionnaire => SyncLogMessages.CensusQuestionnaire.FormatString(censusQuestionnaire.Title,
                    new QuestionnaireIdentity(censusQuestionnaire.QuestionnaireId, censusQuestionnaire.Version)));

            return SyncLogMessages.GetCensusQuestionnaires.FormatString(string.Join("<br>", messagesByCensusQuestionnaires));
        }

        private string GetQuestionnaireLogMessage(string messageFormat, HttpActionExecutedContext context)
        {
            var questionnaire = this.questionnaireBrowseItemFactory.GetById(
                new QuestionnaireIdentity(context.GetActionArgumentOrDefault<Guid>("id", Guid.Empty),
                    context.GetActionArgumentOrDefault<int>("version", -1)));

            if (questionnaire == null)
                return messageFormat.FormatString(UnknownStringArgumentValue, UnknownStringArgumentValue);

            return messageFormat.FormatString(questionnaire.Title, new QuestionnaireIdentity(questionnaire.QuestionnaireId, questionnaire.Version));
        }

        private string GetInterviewerDeviceId()
        {
            return this.userInfoViewFactory.Load(
                new UserWebViewInputModel(this.globalInfoProvider.GetCurrentUser().Name, null)).DeviceId;
        }

        private T GetResponseObject<T>(HttpActionExecutedContext context) where T : class
        {
            var objectContent = context.Response.Content as ObjectContent;
            return (T) objectContent?.Value;
        }
    }
}