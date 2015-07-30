namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Tasks
{
    public class UserPreloadingSettings
    {
        public UserPreloadingSettings(int verificationIntervalInSeconds, int creationIntervalInSeconds, int cleaningIntervalInHours, int howOldInDaysProcessShouldBeInOrderToBeCleaned)
        {
            this.VerificationIntervalInSeconds = verificationIntervalInSeconds;
            this.CreationIntervalInSeconds = creationIntervalInSeconds;
            this.CleaningIntervalInHours = cleaningIntervalInHours;
            this.HowOldInDaysProcessShouldBeInOrderToBeCleaned = howOldInDaysProcessShouldBeInOrderToBeCleaned;
        }

        public int VerificationIntervalInSeconds { get; private set; }
        public int CreationIntervalInSeconds { get; private set; }
        public int CleaningIntervalInHours { get; private set; }
        public int HowOldInDaysProcessShouldBeInOrderToBeCleaned { get; private set; } 
    }
}