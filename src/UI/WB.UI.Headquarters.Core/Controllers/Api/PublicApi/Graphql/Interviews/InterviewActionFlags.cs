namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Interviews
{
    public enum InterviewActionFlags
    {
        CanBeReassigned,
        CanBeDeleted,
        CanBeApproved,
        CanBeUnApprovedByHq,
        CanBeRejected,
        CanBeRestarted,
        CanBeOpened,
        CanChangeToCAPI,
        CanChangeToCAWI
    }
}
