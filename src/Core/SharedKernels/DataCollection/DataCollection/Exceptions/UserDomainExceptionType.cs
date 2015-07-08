namespace WB.Core.SharedKernels.DataCollection.Exceptions
{
    public enum UserDomainExceptionType
    {
        Undefined,
        UserNameUsedByActiveUser,
        UserNameUsedByArchivedUser,
        UserHasAssigments,
        UserArchived,
        UserIsNotArchived,
        RoleDoesntSupportDelete,
        SupervisorArchived
    }
}