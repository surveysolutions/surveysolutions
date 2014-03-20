namespace WB.Core.BoundedContexts.Headquarters.PasswordPolicy
{
    public struct ApplicationPasswordPolicySettings
    {
        public int MinPasswordLength { get; set; }

        public string PasswordPattern { get; set; }
    }
}