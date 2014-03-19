namespace WB.Core.BoundedContexts.Headquarters.Authentication
{
    public struct ApplicationUserManagerSettings
    {
        public int MinPasswordLength { get; set; }

        public string PasswordPattern { get; set; }
    }
}