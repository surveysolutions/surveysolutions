using System;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;

namespace WB.Core.SharedKernels.DataCollection.Events.Questionnaire
{
    [Obsolete]
    public class PlainQuestionnaireRegistered : IEvent
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