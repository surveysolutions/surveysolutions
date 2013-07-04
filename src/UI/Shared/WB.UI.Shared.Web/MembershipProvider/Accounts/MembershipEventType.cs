namespace WB.UI.Shared.Web.MembershipProvider.Accounts
{
    /// <summary>
    /// The membership event type.
    /// </summary>
    public enum MembershipEventType
    {
        /// <summary>
        /// The change password question and answer.
        /// </summary>
        ChangePasswordQuestionAndAnswer, 

        /// <summary>
        /// The change password.
        /// </summary>
        ChangePassword, 

        /// <summary>
        /// The reset password.
        /// </summary>
        ResetPassword, 

        /// <summary>
        /// The change reset password.
        /// </summary>
        ChangePasswordResetToken, 

        /// <summary>
        /// The lock user.
        /// </summary>
        LockUser, 

        /// <summary>
        /// The unlock user.
        /// </summary>
        UnlockUser, 

        /// <summary>
        /// The failed login.
        /// </summary>
        FailedLogin, 

        /// <summary>
        /// The user validated.
        /// </summary>
        UserValidated, 

        /// <summary>
        /// The update online state.
        /// </summary>
        UpdateOnlineState, 

        /// <summary>
        /// The confirm account.
        /// </summary>
        ConfirmAccount, 

        /// <summary>
        /// The update.
        /// </summary>
        Update
    }
}