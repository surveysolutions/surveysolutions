using System;

namespace WB.Core.BoundedContexts.Headquarters.WebInterview
{
    public class TrackingSettings
    {
        public TrackingSettings(TimeSpan pauseResumeGracePeriod)
        {
            PauseResumeGracePeriod = pauseResumeGracePeriod;
        }

        public TimeSpan PauseResumeGracePeriod { get; }

        public TimeSpan DelayBeforeCommandPublish => PauseResumeGracePeriod.Add(TimeSpan.FromSeconds(15));
    }
}