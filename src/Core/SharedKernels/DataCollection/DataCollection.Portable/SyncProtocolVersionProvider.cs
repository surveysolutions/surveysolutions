namespace WB.Core.SharedKernels.DataCollection
{
    public class InterviewerSyncProtocolVersionProvider : IInterviewerSyncProtocolVersionProvider
    {
        public static readonly int ResolvedCommentsIntroduced = 7100;
        public const int WorkspacesIntroduced = 7200;

        //previous values: 5962, 7018, 7034, 7050, 7060, 7070 - where KP-11462 was introduced
        // 7100 before workspaces
        public int GetProtocolVersion() => 7200;

        public int GetLastNonUpdatableVersion() => 7000;
        public int[] GetBlackListedBuildNumbers()
        {
            return new int[]
            {
                26963,  //20.01.3 //options translations issue : KP-13585
                26948,  //20.01.2 //options translations issue : KP-13585
                26886,  //20.01.1 //options translations issue : KP-13585
                26876   //20.01 //options translations issue : KP-13585
            };
        }
    }

    public class SupervisorSyncProtocolVersionProvider : ISupervisorSyncProtocolVersionProvider
    {
        public static readonly int V1_BeforeResolvedCommentsIntroduced = 1_000;
        public static readonly int V2_ResolvedCommentsIntroduced = 1_010;

        public int GetProtocolVersion() => 1_010;

        public int GetLastNonUpdatableVersion() => 999;
        public int[] GetBlackListedBuildNumbers()
        {
            return new int[0];
        }
    }
}
