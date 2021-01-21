using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Domain;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.UI.Headquarters.Resources;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Enumerator.Native.WebInterview;
using WB.Infrastructure.Native.Monitoring;
using WB.Infrastructure.Native.Sanitizer;

namespace WB.UI.Headquarters.Code
{
    [Localizable(false)]
    public class WriteToSyncLogAttribute : ActionFilterAttribute
    {
        private const string UnknownStringArgumentValue = "unknown";
        private readonly SynchronizationLogType logAction;
        private readonly Counter messagesTotal = new Counter(@"wb_hq_sync_log_total_count", @"Count of sync log actions", "action");

        public WriteToSyncLogAttribute(SynchronizationLogType logAction)
        {
            this.logAction = logAction;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var actionExecutedContext = await next();
            IServiceProvider currentContextScope = context.HttpContext.RequestServices;

            IQuestionnaireBrowseViewFactory questionnaireBrowseItemFactory =
                currentContextScope.GetRequiredService<IQuestionnaireBrowseViewFactory>();

            var questionnaireStorage = currentContextScope.GetRequiredService<IQuestionnaireStorage>();
            var answerSerializer = currentContextScope.GetRequiredService<IInterviewAnswerSerializer>();
            var userToDeviceService = currentContextScope.GetRequiredService<IUserToDeviceService>();

            try
            {
                var userIdentity = context.HttpContext.User;
                var userId = userIdentity.UserId();
                var logItem = new SynchronizationLogItem
                {
                    DeviceId = userId.HasValue ? userToDeviceService.GetLinkedDeviceId(userId.Value) : null,
                    InterviewerId = userId ?? Guid.Empty,
                    InterviewerName = userIdentity.UserName() ?? string.Empty,
                    LogDate = DateTime.UtcNow,
                    Type = this.logAction,
                    ActionExceptionType = actionExecutedContext.Exception?.GetType().Name,
                    ActionExceptionMessage = actionExecutedContext.Exception?.Message
                };

                Guid? idAsGuid = Guid.TryParse(context.GetActionArgumentOrDefault<string>("id", string.Empty),
                    out var parsedId) ? parsedId : context.GetActionArgumentOrDefault<Guid?>("id", null);

                var responseStatusCode = actionExecutedContext?.HttpContext.Response.StatusCode;
                switch (this.logAction)
                {
                    case SynchronizationLogType.CanSynchronize:
                        logItem.DeviceId = context.GetActionArgumentOrDefault<string>("deviceId", string.Empty);
                        if (responseStatusCode == StatusCodes.Status200OK)
                            logItem.Log = SyncLogMessages.CanSynchronize;
                        else if (responseStatusCode == StatusCodes.Status426UpgradeRequired)
                        {
                            var version = context.GetActionArgumentOrDefault("version", context.GetActionArgumentOrDefault("deviceSyncProtocolVersion", -1));
                            logItem.Log = SyncLogMessages.DeviceUpdateRequired.FormatString(version);
                        }
                        else if (responseStatusCode == StatusCodes.Status403Forbidden)
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
                        logItem.Log = this.GetInterviewerLogMessage(context, actionExecutedContext);
                        break;
                    case SynchronizationLogType.GetCensusQuestionnaires:
                        logItem.Log = this.GetQuestionnairesLogMessage(actionExecutedContext, questionnaireBrowseItemFactory);
                        break;
                    case SynchronizationLogType.GetQuestionnaire:
                        logItem.Log = this.GetQuestionnaireLogMessage(SyncLogMessages.GetQuestionnaire, context, questionnaireBrowseItemFactory);
                        break;
                    case SynchronizationLogType.QuestionnaireProcessed:
                        logItem.Log = this.GetQuestionnaireLogMessage(SyncLogMessages.QuestionnaireProcessed, context, questionnaireBrowseItemFactory);
                        break;
                    case SynchronizationLogType.GetQuestionnaireAssembly:
                        logItem.Log = this.GetQuestionnaireLogMessage(SyncLogMessages.GetQuestionnaireAssembly, context, questionnaireBrowseItemFactory);
                        break;
                    case SynchronizationLogType.QuestionnaireAssemblyProcessed:
                        logItem.Log = this.GetQuestionnaireLogMessage(SyncLogMessages.QuestionnaireAssemblyProcessed, context, questionnaireBrowseItemFactory);
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
                        logItem.Log = this.GetInterviewsLogMessage(context, actionExecutedContext);
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
                        logItem.Log = this.GetQuestionnaireLogMessage(SyncLogMessages.GetQuestionnaireAttachments, context, questionnaireBrowseItemFactory);
                        break;
                    case SynchronizationLogType.GetAttachmentContent:
                        logItem.Log = SyncLogMessages.GetAttachmentContent.FormatString(context.GetActionArgumentOrDefault<string>("id", string.Empty));
                        break;
                    case SynchronizationLogType.PostInterview:
                        Guid? interviewId = context.GetActionArgumentOrDefault<InterviewPackageApiView>("package", null)?.InterviewId;
                        logItem.Log = SyncLogMessages.PostPackage.FormatString(interviewId.HasValue ? GetInterviewLink(interviewId.Value, context) : UnknownStringArgumentValue);
                        logItem.InterviewId = interviewId;
                        break;
                    case SynchronizationLogType.PostPackage:
                        var packageId = context.GetActionArgumentOrDefault<Guid>("id", Guid.Empty);
                        logItem.Log = SyncLogMessages.PostPackage.FormatString(GetInterviewLink(packageId, context), packageId);
                        logItem.InterviewId = packageId;
                        break;
                    case SynchronizationLogType.GetTranslations:
                        var questionnaireId = context.GetActionArgumentOrDefault<string>("id", null);
                        if (questionnaireId != null)
                        {
                            var questionnaireIdentity = QuestionnaireIdentity.Parse(questionnaireId);
                            var questionnaireInfo = questionnaireBrowseItemFactory.GetById(questionnaireIdentity);
                            logItem.Log = SyncLogMessages.GetTranslations.FormatString(questionnaireInfo.Title, questionnaireInfo.Version);
                        }
                        else
                        {
                            logItem.Log = SyncLogMessages.GetTranslations.FormatString(UnknownStringArgumentValue,
                                UnknownStringArgumentValue);
                        }
                        break;
                    case SynchronizationLogType.InterviewerLogin:
                        var success = responseStatusCode == StatusCodes.Status200OK;
                        logItem.Log = success
                            ? SyncLogMessages.InterviewerLoggedIn
                            : SyncLogMessages.InterviewerFailedToLogin;
                        break;
                    case SynchronizationLogType.GetAssignmentsList:
                        logItem.Log = GetAssignmentsLogMessage(actionExecutedContext, questionnaireStorage);
                        break;
                    case SynchronizationLogType.GetAssignment:
                        logItem.Log = GetAssignmentLogMessage(actionExecutedContext, answerSerializer, questionnaireStorage);
                        break;
                    case SynchronizationLogType.GetMapList:
                        logItem.Log = this.GetMapListLogMessage(actionExecutedContext);
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
                        logItem.Log = SyncLogMessages.GetInterviewPackageV3.FormatString(idAsGuid.HasValue ? GetInterviewLink(idAsGuid.Value, context) : "Null interview id");
                        logItem.InterviewId = idAsGuid;
                        break;
                    case SynchronizationLogType.PostInterviewV3:
                        Guid? intId = context.GetActionArgumentOrDefault<InterviewPackageApiView>("package", null)?.InterviewId;
                        logItem.Log = SyncLogMessages.PostPackageV3.FormatString(intId.HasValue ? GetInterviewLink(intId.Value, context) : UnknownStringArgumentValue);
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
                    case SynchronizationLogType.AssignmentReceived:
                        var assignmentId = context.GetActionArgumentOrDefault<int>("id", 0);
                        logItem.Log = SyncLogMessages.AssignmentReceivedByTablet.FormatString(assignmentId);
                        break;
                    case SynchronizationLogType.DownloadReusableCategories:
                        var categoriesId = context.GetActionArgumentOrDefault<string>("id", string.Empty);
                        logItem.Log = SyncLogMessages.DownloadReusableCategories.FormatString(categoriesId);
                        break;
                    case SynchronizationLogType.GetInterviewPatch:
                        logItem.Log = SyncLogMessages.GetInterviewPatch.FormatString(idAsGuid.HasValue ? GetInterviewLink(idAsGuid.Value, context) : "Null interview id");
                        logItem.InterviewId = idAsGuid;
                        break;
                    
                    case SynchronizationLogType.PostCalendarEvent:
                        Guid? ceId = context.GetActionArgumentOrDefault<CalendarEventPackageApiView>("package", null)?.CalendarEventId;
                        logItem.Log = SyncLogMessages.PostCalendarEventPackage.FormatString(ceId.HasValue ? ceId.ToString() : UnknownStringArgumentValue);
                        logItem.InterviewId = ceId;
                        break;
                    case SynchronizationLogType.GetCalendarEvent:
                        logItem.Log = SyncLogMessages.GetCalendarEventPackage.FormatString(idAsGuid.HasValue ? idAsGuid.ToString() : "Null calendar event id");
                        logItem.InterviewId = idAsGuid;
                        break;
                    case SynchronizationLogType.GetCalendarEvents:
                        logItem.Log = GetCalendarEventsListLogMessage(actionExecutedContext);
                        
                        break;
                    
                    default:
                        throw new ArgumentException(nameof(logAction));
                }

                messagesTotal.Labels(this.logAction.ToString()).Inc();

                var s = currentContextScope.GetRequiredService<IPlainStorageAccessor<SynchronizationLogItem>>();
                s.Store(logItem, Guid.NewGuid());
            }
            catch (Exception exception)
            {
                ILogger logger = (currentContextScope.GetService(typeof(ILoggerProvider)) as ILoggerProvider).GetFor<WriteToSyncLogAttribute>();
                logger.Error($"Error updating sync log on action '{this.logAction}'.", exception);
            }
        }

        private string GetInterviewsLogMessage(ActionExecutingContext executingContext, ActionExecutedContext context)
        {
            var interviewsApiView = this.GetResponseObject<List<InterviewApiView>>(context);

            var messagesByInterviews = interviewsApiView.Select(x => GetInterviewLink(x.Id, executingContext)).ToList();

            var readability = !messagesByInterviews.Any()
                ? SyncLogMessages.NoNewInterviewPackagesToDownload
                : string.Join("<br />", messagesByInterviews);
            return SyncLogMessages.GetInterviews.FormatString(readability);
        }

        private string GetMapListLogMessage(ActionExecutedContext context)
        {
            var mapsApiView = this.GetResponseObject<List<MapView>>(context);

            var readability = !mapsApiView.Any()
                ? SyncLogMessages.NoMapsForUser
                : string.Join("<br />", mapsApiView.Select(x => x.MapName));

            return SyncLogMessages.GetMaps.FormatString(readability);
        }

        private static string GetInterviewLink(Guid interviewId, ActionExecutingContext context)
        {
            var linkGenerator = context.HttpContext.RequestServices.GetRequiredService<LinkGenerator>();

            var interviewLink = linkGenerator.GetUriByAction(context.HttpContext, "Review", "Interview", new { id = interviewId });

            return $"<a href=\"{interviewLink}\">{interviewId:N}</a>";
        }

        private string GetInterviewerLogMessage(ActionExecutingContext executingContext, ActionExecutedContext context)
        {
            IUserViewFactory userViewFactory = executingContext.HttpContext.RequestServices.GetRequiredService<IUserViewFactory>();

            var interviewerApiView = this.GetResponseObject<InterviewerApiView>(context);

            UserView supervisorInfo;
            if (interviewerApiView != null)
            {
                supervisorInfo = userViewFactory.GetUser(new UserViewInputModel(interviewerApiView.SupervisorId));
            }
            else
            {
                supervisorInfo = new UserView
                {
                    UserName = UnknownStringArgumentValue
                };
            }

            return SyncLogMessages.GetInterviewer.FormatString(supervisorInfo.UserName, supervisorInfo.PublicKey.FormatGuid());
        }

        private string GetQuestionnairesLogMessage(ActionExecutedContext context, IQuestionnaireBrowseViewFactory questionnaireBrowseItemFactory)
        {
            List<QuestionnaireIdentity> censusQuestionnaireIdentities =
                this.GetResponseObject<List<QuestionnaireIdentity>>(context);

            var censusQuestionnaires = censusQuestionnaireIdentities.Select(x => questionnaireBrowseItemFactory.GetById(new QuestionnaireIdentity(x.QuestionnaireId, x.Version)));

            var messagesByCensusQuestionnaires = censusQuestionnaires.Select(
                censusQuestionnaire => SyncLogMessages.CensusQuestionnaire.FormatString(censusQuestionnaire.Title,
                    new QuestionnaireIdentity(censusQuestionnaire.QuestionnaireId, censusQuestionnaire.Version)));

            return SyncLogMessages.GetCensusQuestionnaires.FormatString(string.Join("<br>", messagesByCensusQuestionnaires));
        }

        private string GetQuestionnaireLogMessage(string messageFormat, ActionExecutingContext context, IQuestionnaireBrowseViewFactory questionnaireBrowseItemFactory)
        {
            var questionnaire = questionnaireBrowseItemFactory.GetById(
                new QuestionnaireIdentity(context.GetActionArgumentOrDefault("id", Guid.Empty),
                    context.GetActionArgumentOrDefault<int>("version", -1)));

            if (questionnaire == null)
                return messageFormat.FormatString(UnknownStringArgumentValue, UnknownStringArgumentValue);

            return messageFormat.FormatString(questionnaire.Title, new QuestionnaireIdentity(questionnaire.QuestionnaireId, questionnaire.Version));
        }

        private string GetAssignmentsLogMessage(ActionExecutedContext context, IQuestionnaireStorage questionnaireStorage)
        {
            var assignmentsApiView = this.GetResponseObject<List<AssignmentApiView>>(context);

            return SyncLogMessages.GetAssignmentsFormat.FormatString(assignmentsApiView.Count,
                string.Join("", assignmentsApiView.Select(av => $"<li>{GetAssignmentLogMessage(av, questionnaireStorage)}</li>")));
        }

        private string GetCalendarEventsListLogMessage(ActionExecutedContext context)
        {
            var calendarEventsApiView = this.GetResponseObject<List<CalendarEventApiView>>(context);

            return SyncLogMessages.CalendarEventsListFormat.FormatString(calendarEventsApiView.Count,
                string.Join("", calendarEventsApiView.Select(av => $"<li>{av.CalendarEventId}</li>")));
        }
        
        private string GetAssignmentLogMessage(AssignmentApiView view, IQuestionnaireStorage questionnaireStorage)
        {
            QuestionnaireIdentity assignmentQuestionnaireId = view.QuestionnaireId;
            var questionnaire = questionnaireStorage.GetQuestionnaire(assignmentQuestionnaireId, null);
            return $"{view.Id}: <strong>{questionnaire.Title}</strong> [{view.QuestionnaireId}]";
        }

        private string GetAssignmentLogMessage(ActionExecutedContext context, IInterviewAnswerSerializer answerSerializer, IQuestionnaireStorage questionnaireStorage)
        {
            var apiView = this.GetResponseObject<AssignmentApiDocument>(context);
            QuestionnaireIdentity assignmentQuestionnaireId = apiView.QuestionnaireId;
            var questionnaire = questionnaireStorage.GetQuestionnaire(assignmentQuestionnaireId, null);
            var identityQuestions = Enumerable.ToHashSet(questionnaire.GetPrefilledQuestions());

            var answers = apiView.Answers
                .Where(x => !string.IsNullOrEmpty(x.SerializedAnswer))
                .Where(x => identityQuestions.Contains(x.Identity.Id))
                .Select(_ => GetAssignmentIdentifyingQuestionRow(_, questionnaire, answerSerializer));

            var answersString = string.Join("", answers);
            return SyncLogMessages.GetAssignmentFormat.FormatString(
                $"{apiView.Id}: <strong>{questionnaire.Title}</strong> [{apiView.QuestionnaireId}] <ul>{answersString}</ul>");
        }

        private string GetAssignmentIdentifyingQuestionRow(AssignmentApiDocument.InterviewSerializedAnswer _,
            Core.SharedKernels.DataCollection.Aggregates.IQuestionnaire questionnaire,
            IInterviewAnswerSerializer answerSerializer)
        {
            string questionTitle = questionnaire.GetQuestionTitle(_.Identity.Id).RemoveHtmlTags();
            var abstractAnswer = answerSerializer.Deserialize<AbstractAnswer>(_.SerializedAnswer);
            var stringAnswer = abstractAnswer?.ToString();
            return $"<li title='{questionTitle}'>{LimitStringLength(questionTitle)}: {stringAnswer}</li>";
        }

        private string LimitStringLength(string text, int limit = 50)
        {
            if (text.Length > limit)
            {
                return text.Substring(0, limit) + "...";
            }
            return text;
        }

        private T GetResponseObject<T>(ActionExecutedContext context) where T : class
        {
            var objectContent = context?.Result as ObjectResult;
            return (T)objectContent?.Value;
        }
    }
}
