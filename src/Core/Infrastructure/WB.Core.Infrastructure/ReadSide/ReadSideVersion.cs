using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.ReadSide
{
    public class ReadSideVersion : IReadSideRepositoryEntity
    {
        public static readonly string IdOfCurrent = "current";

        public ReadSideVersion(int version)
        {
            this.Version = version;
        }

        public int Version { get; private set; }
    }
}