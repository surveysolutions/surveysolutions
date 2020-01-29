namespace WB.Core.BoundedContexts.Headquarters.Users.UserPreloading
{
    public class UserPreloadingSettings
    {
        public UserPreloadingSettings(
            int maxAllowedRecordNumber, 
            string loginFormatRegex, 
            string emailFormatRegex, 
            string phoneNumberFormatRegex, 
            int fullNameMaxLength, 
            int phoneNumberMaxLength, 
            string personNameFormatRegex)
        {
            this.MaxAllowedRecordNumber = maxAllowedRecordNumber;
            this.LoginFormatRegex = loginFormatRegex;
            this.EmailFormatRegex = emailFormatRegex;
            this.PhoneNumberFormatRegex = phoneNumberFormatRegex;
            this.FullNameMaxLength = fullNameMaxLength;
            this.PhoneNumberMaxLength = phoneNumberMaxLength;
            this.PersonNameFormatRegex = personNameFormatRegex;
        }
        
        public int MaxAllowedRecordNumber { get; private set; }

        public string LoginFormatRegex { get; private set; }
        public string EmailFormatRegex { get; private set; }
        public string PhoneNumberFormatRegex { get; private set; }

        public int FullNameMaxLength { get; private set; }

        public int PhoneNumberMaxLength { get; private set; }
        public string PersonNameFormatRegex { get; private set; }
    }
}
