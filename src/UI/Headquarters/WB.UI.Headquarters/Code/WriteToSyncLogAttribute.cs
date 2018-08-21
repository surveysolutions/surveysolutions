using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Filters;
using Main.Core.Entities.SubEntities;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;

using WB.UI.Headquarters.Resources;
using WB.UI.Headquarters.Utils;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Monitoring;
using WB.Infrastructure.Native.Sanitizer;

namespace WB.UI.Headquarters.Code
{
    [Localizable(false)]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class WriteToSyncLogAttribute : ActionFilterAttribute
    {
        private const string UnknownStringArgumentValue = "unknown";

        private readonly SynchronizationLogType logAction;
        private HqSignInManager signInManager
            => ServiceLocator.Current.GetInstance<HqSignInManager>();

        private IPlainStorageAccessor<SynchronizationLogItem> synchronizationLogItemPlainStorageAccessor
            => ServiceLocator.Current.GetInstance<IPlainStorageAccessor<SynchronizationLogItem>>();

        private IAuthorizedUser authorizedUser => ServiceLocator.Current.GetInstance<IAuthorizedUser>();

        private IQuestionnaireBrowseViewFactory questionnaireBrowseItemFactory => ServiceLocator.Current.GetInstance<IQuestionnaireBrowseViewFactory>();
        private IQuestionnaireStorage questionnaireStorage => ServiceLocator.Current.GetInstance<IQuestionnaireStorage>();

        private IUserViewFactory userViewFactory => ServiceLocator.Current.GetInstance<IUserViewFactory>();
        private IInterviewAnswerSerializer answerSerializer => ServiceLocator.Current.GetInstance<IInterviewAnswerSerializer>();

        private ILogger logger => ServiceLocator.Current.GetInstance<ILoggerProvider>().GetFor<WriteToSyncLogAttribute>();

        public WriteToSyncLogAttribute(SynchronizationLogType logAction)
        {
            this.logAction = logAction;
        }

        public override async Task OnActionExecutedAsync(HttpActionExecutedContext context, CancellationToken cancellationToken)
        {
            await base.OnActionExecutedAsync(context, cancellationToken);

            try
            {
                if (!this.authorizedUser.IsAuthenticated)
                {
                    var authHeader = context.Request?.Headers?.Authorization?.ToString();

                    if (authHeader != null)
                        await signInManager.SignInWithAuthTokenAsync(authHeader, false, UserRoles.Interviewer);
                }

                var logItem = new SynchronizationLogItem
                {
                    DeviceId = this.authorizedUser.IsAuthenticated ? this.authorizedUser.DeviceId : null,
                    InterviewerId = this.authorizedUser.IsAuthenticated ? this.authorizedUser.Id : Guid.Empty,
                    InterviewerName = this.authorizedUser.IsAuthenticated ? this.authorizedUser.UserName : string.Empty, 
                    LogDate = DateTime.UtcNow,
                    Type = this.logAction
                };

                Guid? idAsGuid = Guid.TryParse(context.GetActionArgumentOrDefault<string>("id", string.Empty), 
                    out var parsedId) ? parsedId : context.GetActionArgumentOrDefault<Guid?>("id", null);

                switch (this.logAction)
                {
                    case SynchronizationLogType.CanSynchronize:
                        logItem.DeviceId = context.GetActionArgumentOrDefault<string>("deviceId", string.Empty);
                        if (context.Response.IsSuccessStatusCode) 
                            logItem.Log = SyncLogMessages.CanSynchronize;
                        else if (context.Response.StatusCode == HttpStatusCode.UpgradeRequired)
                        {
                            var version = context.GetActionArgumentOrDefault("version", context.GetActionArgumentOrDefault("deviceSyncProtocolVersion", -1));
                            logItem.Log =  SyncLogMessages.DeviceUpdateRequired.FormatString(version);
                        }
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
                        logItem.InterviewId = idAsGuid;
                        break;
                    case SynchronizationLogType.InterviewPackageProcessed:
                        logItem.Log = SyncLogMessages.InterviewPackageProcessed.FormatString(context.GetActionArgumentOrDefault<string>("id", string.Empty));
                        logItem.InterviewId = idAsGuid;
                        break;
                    case SynchronizationLogType.GetInterviews:
                        logItem.Log = this.GetInterviewsLogMessage(context);
                        break;
                    case SynchronizationLogType.GetInterview:
                        logItem.Log = SyncLogMessages.GetInterview.FormatString(idAsGuid);
                        logItem.InterviewId = idAsGuid;
                        break;
                    case SynchronizationLogType.InterviewProcessed:
                        logItem.Log = SyncLogMessages.InterviewProcessed.FormatString(idAsGuid);
                        logItem.InterviewId = idAsGuid;
                        break;
                    case SynchronizationLogType.GetQuestionnaireAttachments:
                        logItem.Log = this.GetQuestionnaireLogMessage(SyncLogMessages.GetQuestionnaireAttachments, context);
                        break;
                    case SynchronizationLogType.GetAttachmentContent:
                        logItem.Log = SyncLogMessages.GetAttachmentContent.FormatString(context.GetActionArgumentOrDefault<string>("id", string.Empty));
                        break;
                    case SynchronizationLogType.PostInterview:
                        Guid? interviewId = context.GetActionArgumentOrDefault<InterviewPackageApiView>("package", null)?.InterviewId;
                        logItem.Log = SyncLogMessages.PostPackage.FormatString(interviewId.HasValue ? GetInterviewLink(interviewId.Value) : UnknownStringArgumentValue);
                        logItem.InterviewId = interviewId;
                        break;
                    case SynchronizationLogType.PostPackage:
                        var packageId = context.GetActionArgumentOrDefault<Guid>("id", Guid.Empty);
                        logItem.Log = SyncLogMessages.PostPackage.FormatString(GetInterviewLink(packageId), packageId);
                        logItem.InterviewId = packageId;
                        break;
                    case SynchronizationLogType.GetTranslations:
                        var questionnaireId = context.GetActionArgumentOrDefault<string>("id", null);
                        if (questionnaireId != null)
                        {
                            var questionnaireIdentity = QuestionnaireIdentity.Parse(questionnaireId);
                            var questionnaireInfo = this.questionnaireBrowseItemFactory.GetById(questionnaireIdentity);
                            logItem.Log = SyncLogMessages.GetTranslations.FormatString(questionnaireInfo.Title, questionnaireInfo.Version);
                        }
                        else
                        {
                            logItem.Log = SyncLogMessages.GetTranslations.FormatString(UnknownStringArgumentValue,
                                UnknownStringArgumentValue);
                        }
                        break;
                    case SynchronizationLogType.InterviewerLogin:
                        var success = context.Response.IsSuccessStatusCode;
                        logItem.Log = success 
                            ? SyncLogMessages.InterviewerLoggedIn
                            : SyncLogMessages.InterviewerFailedToLogin;
                        break;
                    case SynchronizationLogType.GetAssignmentsList:
                        logItem.Log = GetAssignmentsLogMessage(context);
                        break;
                    case SynchronizationLogType.GetAssignment:
                        logItem.Log = GetAssignmentLogMessage(context);
                        break;
                    case SynchronizationLogType.GetMapList:
                        logItem.Log = this.GetMapListLogMessage(context);
                        break;
                    case SynchronizationLogType.GetMap:
                        logItem.Log = SyncLogMessages.GetMap.FormatString(context.GetActionArgumentOrDefault<string>("id", string.Empty));
                        break;
                    case SynchronizationLogType.GetApk:
                        logItem.Log = SyncLogMessages.ApkRequested;
                        break;
                    case SynchronizationLogType.GetSupervisorApk:
                        logItem.Log = SyncLogMessages.SupervisorApkRequested;
                        break;
                    case SynchronizationLogType.GetExtendedApk:
                        logItem.Log = SyncLogMessages.ExtendedApkRequested;
                        break;
                    case SynchronizationLogType.GetApkPatch:
                        logItem.Log = SyncLogMessages.PatchRequestedFormat.FormatString(context.GetActionArgumentOrDefault<string>("deviceVersion", string.Empty));
                        break;
                    case SynchronizationLogType.GetSupervisorApkPatch:
                        logItem.Log = SyncLogMessages.SupervisorPatchRequestedFormat.FormatString(context.GetActionArgumentOrDefault<string>("deviceVersion", string.Empty));
                        break;
                    case SynchronizationLogType.GetExtendedApkPatch:
                        logItem.Log = SyncLogMessages.ExtendedPatchRequestedFormat.FormatString(context.GetActionArgumentOrDefault<string>("deviceVersion", string.Empty));
                        break;
                    case SynchronizationLogType.GetInterviewV3:
                        logItem.Log = SyncLogMessages.GetInterviewPackageV3.FormatString(idAsGuid.HasValue ? GetInterviewLink(idAsGuid.Value) : "Null interview id");
                        logItem.InterviewId = idAsGuid;
                        break;
                    case SynchronizationLogType.PostInterviewV3:
                        Guid? intId = context.GetActionArgumentOrDefault<InterviewPackageApiView>("package", null)?.InterviewId;
                        logItem.Log = SyncLogMessages.PostPackageV3.FormatString(intId.HasValue ? GetInterviewLink(intId.Value) : UnknownStringArgumentValue);
                        logItem.InterviewId = intId;
                        break;
                    case SynchronizationLogType.CheckObsoleteInterviews:
                        var request = context.GetActionArgumentOrDefault("knownPackages", new List<ObsoletePackageCheck>());
                        logItem.Log = string.Format(SyncLogMessages.CheckObsoleteInterviews, JsonConvert.SerializeObject(request, Formatting.Indented));
                        break;
                    case SynchronizationLogType.CheckIsPackageDuplicated:
                        var duplicatedPackageCheckId = context.GetActionArgumentOrDefault<Guid>("id", Guid.Empty);
                        logItem.Log = SyncLogMessages.CheckIsPackageDuplicatedFormat.FormatString(duplicatedPackageCheckId.ToString());
                        logItem.InterviewId = duplicatedPackageCheckId;
                        break;
                    case SynchronizationLogType.GetInterviewerAppPatches:
                        logItem.Log = "Get list of interviewer app patches";
                        break;
                    case SynchronizationLogType.GetInterviewerAppPatchByName:
                        logItem.Log = $"Get interviewer app patch: {context.GetActionArgumentOrDefault("id", string.Empty)}";
                        break;
                    default:
                        throw new ArgumentException("logAction");
                }

                messagesTotal.Labels(this.logAction.ToString()).Inc();
                this.synchronizationLogItemPlainStorageAccessor.Store(logItem, Guid.NewGuid());

            }
            catch (Exception exception)
            {
                this.logger.Error($"Error updating sync log on action '{this.logAction}'.", exception);
            }
        }

        private readonly Counter messagesTotal = new Counter(@"wb_hq_sync_log_total_count", @"Count of sync log actions", "action");


        private string GetInterviewsLogMessage(HttpActionExecutedContext context)
        {
            var interviewsApiView = this.GetResponseObject<List<InterviewApiView>>(context);

            var messagesByInterviews = interviewsApiView.Select(x => GetInterviewLink(x.Id)).ToList();

            var readability = !messagesByInterviews.Any()
                ? SyncLogMessages.NoNewInterviewPackagesToDownload
                : string.Join("<br />", messagesByInterviews);
            return SyncLogMessages.GetInterviews.FormatString(readability);
        }

        private string GetMapListLogMessage(HttpActionExecutedContext context)
        {
            var mapsApiView = this.GetResponseObject<List<MapView>>(context);

            var readability = !mapsApiView.Any()
                ? SyncLogMessages.NoMapsForUser
                : string.Join("<br />", mapsApiView.Select(x => x.MapName));

            return SyncLogMessages.GetMaps.FormatString(readability);
        }

        private static string GetInterviewLink(Guid interviewId)
        {
            var interviewLink = new System.Web.Mvc.UrlHelper(HttpContext.Current.Request.RequestContext).Action("Review",
                new { controller = "Interview", action = "Review", id = interviewId });

            return $"<a href=\"{interviewLink}\">{interviewId:N}</a>";
        }

        private string GetInterviewerLogMessage(HttpActionExecutedContext context)
        {
            var interviewerApiView = this.GetResponseObject<InterviewerApiView>(context);

            var supervisorInfo = this.userViewFactory.GetUser(new UserViewInputModel(interviewerApiView.SupervisorId));
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
                new QuestionnaireIdentity(context.GetActionArgumentOrDefault("id", Guid.Empty),
                    context.GetActionArgumentOrDefault<int>("version", -1)));

            if (questionnaire == null)
                return messageFormat.FormatString(UnknownStringArgumentValue, UnknownStringArgumentValue);

            return messageFormat.FormatString(questionnaire.Title, new QuestionnaireIdentity(questionnaire.QuestionnaireId, questionnaire.Version));
        }

        private string GetAssignmentsLogMessage(HttpActionExecutedContext context)
        {
            var assignmentsApiView = this.GetResponseObject<List<AssignmentApiView>>(context);
            
            return SyncLogMessages.GetAssignmentsFormat.FormatString(assignmentsApiView.Count,
                string.Join("", assignmentsApiView.Select(av => $"<li>{GetAssignmentLogMessage(av)}</li>")));
        }

        private string GetAssignmentLogMessage(AssignmentApiView view)
        {
            QuestionnaireIdentity assignmentQuestionnaireId = view.QuestionnaireId;
            var questionnaire = this.questionnaireStorage.GetQuestionnaire(assignmentQuestionnaireId, null);
            return $"{view.Id}: <strong>{questionnaire.Title}</strong> [{view.QuestionnaireId}]";
        }

        private string GetAssignmentLogMessage(HttpActionExecutedContext context)
        {
            var apiView = this.GetResponseObject<AssignmentApiDocument>(context);
            QuestionnaireIdentity assignmentQuestionnaireId = apiView.QuestionnaireId;
            var questionnaire = this.questionnaireStorage.GetQuestionnaire(assignmentQuestionnaireId, null);
            var identityQuestions = questionnaire.GetPrefilledQuestions().ToHashSet();

            var answers = apiView.Answers
                .Where(x => !string.IsNullOrEmpty(x.SerializedAnswer))
                .Where(x => identityQuestions.Contains(x.Identity.Id))
                .Select(_ => GetAssignmentIdentifyingQuestionRow(_, questionnaire));

            var answersString = string.Join("", answers);
            return SyncLogMessages.GetAssignmentFormat.FormatString(
                $"{apiView.Id}: <strong>{questionnaire.Title}</strong> [{apiView.QuestionnaireId}] <ul>{answersString}</ul>");
        }

        private string GetAssignmentIdentifyingQuestionRow(AssignmentApiDocument.InterviewSerializedAnswer _, Core.SharedKernels.DataCollection.Aggregates.IQuestionnaire questionnaire)
        {
            string questionTitle = questionnaire.GetQuestionTitle(_.Identity.Id).RemoveHtmlTags();
            var abstractAnswer = this.answerSerializer.Deserialize<AbstractAnswer>(_.SerializedAnswer);
            var stringAnswer = abstractAnswer?.ToString();
            return $"<li title='{questionTitle}'>{LimitStringLength(questionTitle)}: {stringAnswer}</li>";
        }

        private string LimitStringLength(string text, int limit = 50)
        {
            if (text.Length > limit) {
                return text.Substring(0, limit) + "...";
            }
            return text;
        }

        private T GetResponseObject<T>(HttpActionExecutedContext context) where T : class
        {
            var objectContent = context.Response.Content as ObjectContent;
            return (T) objectContent?.Value;
        }
    }
}
