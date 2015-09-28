using System;
using System.Web.Http.Filters;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Code
{
    public enum SyncLogAction
    {
        MarkQuestionnaireAsSuccessfullyHandled,
        TrackCurrentUserRequest,
        TrackQuestionnaireRequest,
        TrackAggregateRootIdsRequest,
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class WriteToSyncLogAttribute : ActionFilterAttribute
    {
        private readonly SyncLogAction logAction;
        private readonly string synchronizationItemType;

        public ISyncLogger SyncLogger
        {
            get { return ServiceLocator.Current.GetInstance<ISyncLogger>(); }
        }

        public IGlobalInfoProvider GlobalInfoProvider
        {
            get { return ServiceLocator.Current.GetInstance<IGlobalInfoProvider>(); }
        }

        public IUserWebViewFactory UserInfoViewFactory
        {
            get { return ServiceLocator.Current.GetInstance<IUserWebViewFactory>(); }
        }

        public ILogger Logger
        {
            get { return ServiceLocator.Current.GetInstance<ILogger>(); }
        }

        public WriteToSyncLogAttribute(SyncLogAction logAction, string synchronizationItemType)
        {
            this.logAction = logAction;
            this.synchronizationItemType = synchronizationItemType;
        }

        public override void OnActionExecuted(HttpActionExecutedContext context)
        {
            base.OnActionExecuted(context);

            try
            {
                switch (this.logAction)
                {
                    case SyncLogAction.TrackAggregateRootIdsRequest:
                        this.TrackAggregateRootIdsRequest();
                        break;

                    case SyncLogAction.TrackCurrentUserRequest:
                        this.TrackCurrentUserRequest();
                        break;

                    case SyncLogAction.TrackQuestionnaireRequest:
                        this.TrackQuestionnaireRequest(context);
                        break;

                    case SyncLogAction.MarkQuestionnaireAsSuccessfullyHandled:
                        this.MarkQuestionnaireAsSuccessfullyHandled(context);
                        break;

                    default:
                        throw new ArgumentException("logAction");
                }
            }
            catch (Exception exception)
            {
                this.Logger.Error("Error updating sync log.", exception);
            }
        }

        private void TrackAggregateRootIdsRequest()
        {
            this.SyncLogger.TrackArIdsRequest(
                this.GetInterviewerDeviceId(),
                this.GlobalInfoProvider.GetCurrentUser().Id,
                this.synchronizationItemType,
                new[] { "all" });
        }

        private void TrackQuestionnaireRequest(HttpActionExecutedContext context)
        {
            var id = context.GetActionArgument<Guid>("id");
            var version = context.GetActionArgument<int>("version");

            this.SyncLogger.TrackPackageRequest(
                this.GetInterviewerDeviceId(),
                this.GlobalInfoProvider.GetCurrentUser().Id,
                this.GetSyncLogQuestionnaireId(id, version));
        }

        private void TrackCurrentUserRequest()
        {
            this.SyncLogger.TrackPackageRequest(
                this.GetInterviewerDeviceId(),
                this.GlobalInfoProvider.GetCurrentUser().Id,
                this.GetSyncLogUserId(this.GlobalInfoProvider.GetCurrentUser().Id));
        }

        private void MarkQuestionnaireAsSuccessfullyHandled(HttpActionExecutedContext context)
        {
            var id = context.GetActionArgument<Guid>("id");
            var version = context.GetActionArgument<int>("version");

            this.SyncLogger.MarkPackageAsSuccessfullyHandled(
                this.GetInterviewerDeviceId(),
                this.GlobalInfoProvider.GetCurrentUser().Id,
                this.GetSyncLogQuestionnaireId(id, version));
        }

        private Guid GetInterviewerDeviceId()
        {
            return this.UserInfoViewFactory.Load(
                new UserWebViewInputModel(this.GlobalInfoProvider.GetCurrentUser().Name, null)).DeviceId.ToGuid();
        }

        private string GetSyncLogQuestionnaireId(Guid questionnaireId, long questionnaireVersion)
        {
            return string.Format("{0}${1}", this.synchronizationItemType, new QuestionnaireIdentity(questionnaireId, questionnaireVersion));
        }

        private string GetSyncLogUserId(Guid userId)
        {
            return string.Format("{0}${1}", this.synchronizationItemType, userId.FormatGuid());
        }
    }
}