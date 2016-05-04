namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto
{
    public enum UserPrelodingState
    {
        Uploaded,
        ReadyForValidation,
        Validating,
        Validated,
        ReadyForUserCreation,
        CreatingUsers,
        Finished,
        FinishedWithError,
        ValidationFinishedWithError
    }
}