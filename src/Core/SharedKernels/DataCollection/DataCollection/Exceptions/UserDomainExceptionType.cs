namespace WB.Core.SharedKernels.DataCollection.Exceptions
{
    public enum UserDomainExceptionType
    {
        Undefined,
        UserNameUsedByActiveUser,
        UserNameUsedByArchivedUser,
        UserArchived,
        UserIsNotArchived,
        RoleDoesntSupportDelete,
        SupervisorArchived
    }
}