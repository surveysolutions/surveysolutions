namespace WB.Core.SharedKernels.DataCollection
{
    public class InterviewerSyncProtocolVersionProvider : IInterviewerSyncProtocolVersionProvider
    {
        public static readonly int ProtectedVariablesIntroduced = 7070;
        public static readonly int AudioRecordingIntroduced = 7090;

        //previous values: 5962, 7018, 7034, 7050, 7060, 7070 - where KP-11462 was introduced 
        public int GetProtocolVersion() => 7090;

        public int GetLastNonUpdatableVersion() => 7000;
    }

    public class SupervisorSyncProtocolVersionProvider : ISupervisorSyncProtocolVersionProvider
    {
        public int GetProtocolVersion() => 1000;

        public int GetLastNonUpdatableVersion() => 999;
    }
}
