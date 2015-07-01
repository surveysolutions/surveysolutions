namespace WB.Core.SharedKernels.DataCollection.Exceptions
{
    public enum UserDomainExceptionType
    {
        Undefined,
        UserNameTakenByActiveUsers,
        UserNameTakenByArchivedUsers,
        UserHasAssigments,
        UserArchived,
        UserIsNotArchived,
        RoleDoesntSupportDelete,
        SupervisorArchived
    }
}