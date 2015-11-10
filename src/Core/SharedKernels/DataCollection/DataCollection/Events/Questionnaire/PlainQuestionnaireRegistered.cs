using WB.Core.Infrastructure.EventBus.Lite;

namespace WB.Core.SharedKernels.DataCollection.Events.Questionnaire
{
    public class PlainQuestionnaireRegistered : ILiteEvent
    {
        public long Version { get; private set; }
        public bool AllowCensusMode { get; private set; }

        public PlainQuestionnaireRegistered(long version, bool allowCensusMode)
        {
            this.AllowCensusMode = allowCensusMode;
            this.Version = version;
        }
    }
}