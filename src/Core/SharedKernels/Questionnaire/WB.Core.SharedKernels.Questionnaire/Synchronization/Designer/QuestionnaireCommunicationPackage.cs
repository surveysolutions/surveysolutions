using WB.Core.SharedKernels.SurveySolutions.Api.Designer;

namespace WB.Core.SharedKernel.Structures.Synchronization.Designer
{
    public class QuestionnaireCommunicationPackage
    {
        public QuestionnaireCommunicationPackage(string questionnaire, string questionnaireAssembly, long questionnaireContentVersion)
        {
            Questionnaire = questionnaire;
            QuestionnaireAssembly = questionnaireAssembly;
            QuestionnaireContentVersion = questionnaireContentVersion;
        }

        public string Questionnaire { get; set; }
        public string QuestionnaireAssembly { get; set; }
        public long QuestionnaireContentVersion { get; set; }
    }
}
