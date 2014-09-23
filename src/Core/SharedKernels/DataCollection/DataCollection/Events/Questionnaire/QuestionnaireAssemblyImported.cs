namespace WB.Core.SharedKernels.DataCollection.Events.Questionnaire
{
    public class QuestionnaireAssemblyImported
    {
        public string AssemblySourceInBase64 { get; set; }
        public long Version { get; set; }
    }
}
