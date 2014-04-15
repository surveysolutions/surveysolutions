namespace WB.Core.SharedKernels.DataCollection.Events.Questionnaire
{
    public class PlainQuestionnaireRegistered
    {
        public long Version { get; private set; }

        public PlainQuestionnaireRegistered(long version)
        {
            this.Version = version;
        }
    }
}