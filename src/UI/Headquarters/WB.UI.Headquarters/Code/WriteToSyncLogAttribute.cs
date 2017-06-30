using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Filters;
using WB.Core.BoundedContexts.Headquarters.Factories;
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
using WB.Infrastructure.Native.Sanitizer;

namespace WB.UI.Headquarters.Code
{
    [Localizable(false)]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class WriteToSyncLogAttribute : ActionFilterAttribute
    {
        private const string UnknownStringArgumentValue = "unknown";

        private readonly SynchronizationLogType logAction;

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

        public override void OnActionExecuted(HttpActionExecutedContext context)
        {
            base.OnActionExecuted(context);

            try
            {
                var logItem = new SynchronizationLogItem
                {
                    DeviceId = this.authorizedUser.IsAuthenticated ? this.authorizedUser.DeviceId : null,
                    InterviewerId = this.authorizedUser.IsAuthenticated ? this.authorizedUser.Id : Guid.Empty,
                    InterviewerName = this.authorizedUser.IsAuthenticated ? this.authorizedUser.UserName : string.Empty, 
                    LogDate = DateTime.UtcNow,
                    Type = this.logAction
                };

                Guid parsedId;
                Guid? idAsGuid = Guid.TryParse(context.GetActionArgumentOrDefault<string>("id", string.Empty), out parsedId) ? parsedId : context.GetActionArgumentOrDefault<Guid?>("id", null);

                switch (this.logAction)
                {
                    case SynchronizationLogType.CanSynchronize:
                        logItem.DeviceId = context.GetActionArgumentOrDefault<string>("deviceId", string.Empty);
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
                    default:
                        throw new ArgumentException("logAction");
                }
                this.synchronizationLogItemPlainStorageAccessor.Store(logItem, Guid.NewGuid());
            }
            catch (Exception exception)
            {
                this.logger.Error($"Error updating sync log on action '{this.logAction}'.", exception);
            }
        }

        private string GetInterviewsLogMessage(HttpActionExecutedContext context)
        {
            var interviewsApiView = this.GetResponseObject<List<InterviewApiView>>(context);

            var messagesByInterviews = interviewsApiView.Select(x => GetInterviewLink(x.Id)).ToList();

            var readability = !messagesByInterviews.Any()
                ? SyncLogMessages.NoNewInterviewPackagesToDownload
                : string.Join("<br />", messagesByInterviews);
            return SyncLogMessages.GetInterviews.FormatString(readability);
        }

        private static string GetInterviewLink(Guid interviewId)
        {
            var interviewLink = new System.Web.Mvc.UrlHelper(HttpContext.Current.Request.RequestContext).Action("Details",
                new { controller = "Interview", action = "Details", id = interviewId });

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