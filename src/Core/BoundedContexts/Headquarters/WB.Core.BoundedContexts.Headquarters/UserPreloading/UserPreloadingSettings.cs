namespace WB.Core.BoundedContexts.Headquarters.UserPreloading
{
    public class UserPreloadingSettings
    {
        public UserPreloadingSettings(
            int executionIntervalInSeconds, 
            int maxAllowedRecordNumber, 
            string loginFormatRegex, 
            string emailFormatRegex, 
            string passwordFormatRegex, 
            string phoneNumberFormatRegex, 
            int fullNameMaxLength, 
            int phoneNumberMaxLength, 
            string personNameFormatRegex)
        {
            this.ExecutionIntervalInSeconds = executionIntervalInSeconds;
            this.MaxAllowedRecordNumber = maxAllowedRecordNumber;
            this.LoginFormatRegex = loginFormatRegex;
            this.EmailFormatRegex = emailFormatRegex;
            this.PasswordFormatRegex = passwordFormatRegex;
            this.PhoneNumberFormatRegex = phoneNumberFormatRegex;
            this.FullNameMaxLength = fullNameMaxLength;
            this.PhoneNumberMaxLength = phoneNumberMaxLength;
            this.PersonNameFormatRegex = personNameFormatRegex;
        }
        
        public int ExecutionIntervalInSeconds { get; private set; }
        public int MaxAllowedRecordNumber { get; private set; }

        public string LoginFormatRegex { get; private set; }
        public string EmailFormatRegex { get; private set; }
        public string PasswordFormatRegex { get; private set; }
        public string PhoneNumberFormatRegex { get; private set; }

        public int FullNameMaxLength { get; private set; }

        public int PhoneNumberMaxLength { get; private set; }
        public string PersonNameFormatRegex { get; private set; }
    }
}