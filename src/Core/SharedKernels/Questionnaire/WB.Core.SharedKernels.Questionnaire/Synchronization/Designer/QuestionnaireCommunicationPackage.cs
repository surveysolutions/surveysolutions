using WB.Core.SharedKernels.SurveySolutions.Api.Designer;

namespace WB.Core.SharedKernel.Structures.Synchronization.Designer
{
    public class QuestionnaireCommunicationPackage
    {
        public QuestionnaireCommunicationPackage(string questionnaire, string questionnaireAssembly,
            long questionnaireContentVersion, bool hasCriticalityCheck)
        {
            Questionnaire = questionnaire;
            QuestionnaireAssembly = questionnaireAssembly;
            QuestionnaireContentVersion = questionnaireContentVersion;
            HasCriticalityCheck = hasCriticalityCheck;
        }

        public bool? HasCriticalityCheck { get; set; }

        public string Questionnaire { get; set; }
        public string QuestionnaireAssembly { get; set; }
        public long QuestionnaireContentVersion { get; set; }
    }
}
