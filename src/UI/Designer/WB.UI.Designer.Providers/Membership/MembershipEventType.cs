
namespace Designer.Web.Providers.Membership
{
    public enum MembershipEventType
    {
        ChangePasswordQuestionAndAnswer,
        ChangePassword,
        ResetPassword,
        LockUser,
        UnlockUser,
        FailedLogin,
        UserValidated,
        UpdateOnlineState,
        ConfirmAccount,
        Update
    }
}
