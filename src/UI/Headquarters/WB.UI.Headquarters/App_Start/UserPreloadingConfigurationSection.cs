using System.Configuration;

namespace WB.UI.Headquarters
{
    public class UserPreloadingConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("verificationIntervalInSeconds", DefaultValue = 5)]
        public int VerificationIntervalInSeconds
        {
            get { return (int) this["verificationIntervalInSeconds"]; }
        }

        [ConfigurationProperty("creationIntervalInSeconds", DefaultValue = 5)]
        public int CreationIntervalInSeconds
        {
            get { return (int) this["creationIntervalInSeconds"]; }
        }

        [ConfigurationProperty("cleaningIntervalInHours", DefaultValue = 12)]
        public int CleaningIntervalInHours
        {
            get { return (int) this["cleaningIntervalInHours"]; }
        }

        [ConfigurationProperty("howOldInDaysProcessShouldBeInOrderToBeCleaned", DefaultValue = 1)]
        public int HowOldInDaysProcessShouldBeInOrderToBeCleaned
        {
            get { return (int) this["howOldInDaysProcessShouldBeInOrderToBeCleaned"]; }
        }

        [ConfigurationProperty("maxAllowedRecordNumber", DefaultValue = 10000)]
        public int MaxAllowedRecordNumber
        {
            get { return (int) this["maxAllowedRecordNumber"]; }
        }

        [ConfigurationProperty("numberOfRowsToBeVerifiedInOrderToUpdateVerificationProgress", DefaultValue = 100)]
        public int NumberOfRowsToBeVerifiedInOrderToUpdateVerificationProgress
        {
            get { return (int) this["numberOfRowsToBeVerifiedInOrderToUpdateVerificationProgress"]; }
        }

        [ConfigurationProperty("numberOfValidationErrorsBeforeStopValidation", DefaultValue = 100)]
        public int NumberOfValidationErrorsBeforeStopValidation
        {
            get { return (int) this["numberOfValidationErrorsBeforeStopValidation"]; }
        }

        [ConfigurationProperty("emailFormatRegex",
            DefaultValue =
                @"^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$"
            )]
        public string EmailFormatRegex
        {
            get { return (string) this["emailFormatRegex"]; }
        }

        [ConfigurationProperty("phoneNumberFormatRegex",
            DefaultValue =
                @"^(\+\s?)?((?<!\+.*)\(\+?\d+([\s\-\.]?\d+)?\)|\d+)([\s\-\.]?(\(\d+([\s\-\.]?\d+)?\)|\d+))*(\s?(x|ext\.?)\s?\d+)?$"
            )]
        public string PhoneNumberFormatRegex
        {
            get { return (string) this["phoneNumberFormatRegex"]; }
        }
    }
}