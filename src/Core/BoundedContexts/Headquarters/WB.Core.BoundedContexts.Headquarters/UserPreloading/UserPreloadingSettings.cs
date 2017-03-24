namespace WB.Core.BoundedContexts.Headquarters.UserPreloading
{
    public class UserPreloadingSettings
    {
        public UserPreloadingSettings(
            int verificationIntervalInSeconds, 
            int creationIntervalInSeconds, 
            int cleaningIntervalInHours, 
            int howOldInDaysProcessShouldBeInOrderToBeCleaned, 
            int maxAllowedRecordNumber, 
            int numberOfRowsToBeVerifiedInOrderToUpdateVerificationProgress, 
            int numberOfValidationErrorsBeforeStopValidation, 
            string loginFormatRegex, 
            string emailFormatRegex, 
            string passwordFormatRegex, 
            string phoneNumberFormatRegex, 
            int fullNameMaxLength, 
            int phoneNumberMaxLength)
        {
            this.VerificationIntervalInSeconds = verificationIntervalInSeconds;
            this.CreationIntervalInSeconds = creationIntervalInSeconds;
            this.CleaningIntervalInHours = cleaningIntervalInHours;
            this.HowOldInDaysProcessShouldBeInOrderToBeCleaned = howOldInDaysProcessShouldBeInOrderToBeCleaned;
            this.MaxAllowedRecordNumber = maxAllowedRecordNumber;
            this.NumberOfRowsToBeVerifiedInOrderToUpdateVerificationProgress = numberOfRowsToBeVerifiedInOrderToUpdateVerificationProgress;
            this.NumberOfValidationErrorsBeforeStopValidation = numberOfValidationErrorsBeforeStopValidation;
            this.LoginFormatRegex = loginFormatRegex;
            this.EmailFormatRegex = emailFormatRegex;
            this.PasswordFormatRegex = passwordFormatRegex;
            this.PhoneNumberFormatRegex = phoneNumberFormatRegex;
            this.FullNameMaxLength = fullNameMaxLength;
            this.PhoneNumberMaxLength = phoneNumberMaxLength;
        }

        public int VerificationIntervalInSeconds { get; private set; }
        public int CreationIntervalInSeconds { get; private set; }
        public int CleaningIntervalInHours { get; private set; }
        public int HowOldInDaysProcessShouldBeInOrderToBeCleaned { get; private set; }
        public int MaxAllowedRecordNumber { get; private set; }
        public int NumberOfRowsToBeVerifiedInOrderToUpdateVerificationProgress { get; private set; }
        public int NumberOfValidationErrorsBeforeStopValidation { get; private set; }

        public string LoginFormatRegex { get; private set; }
        public string EmailFormatRegex { get; private set; }
        public string PasswordFormatRegex { get; private set; }
        public string PhoneNumberFormatRegex { get; private set; }

        public int FullNameMaxLength { get; private set; }

        public int PhoneNumberMaxLength { get; private set; }
    }
}