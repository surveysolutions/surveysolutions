using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    internal class DesignerEngineVersionService : IDesignerEngineVersionService
    {
        private const int OldestQuestionnaireContentVersion = 16;
        private class QuestionnaireContentVersion
        {
            public int Version { get; set; }
            public IEnumerable<QuestionnaireFeature> NewFeatures { get; set; } 
        }

        private class QuestionnaireFeature
        {
            public string Description { get; set; }
            public Func<QuestionnaireDocument, bool> HasQuestionnaire { get; set; }
        }

        private readonly List<QuestionnaireContentVersion> questionnaireContentVersions = new List<QuestionnaireContentVersion>
        {
            new QuestionnaireContentVersion
            {
                Version = ApiVersion.MaxQuestionnaireVersion, /*When adding new version, it should be changed to previous value of ApiVersion.MaxQuestionnaireVersion*/
                NewFeatures = new []
                {
                    new QuestionnaireFeature
                    {
                        HasQuestionnaire = questionnaire => true,
                        Description = "Performance improvements"
                    }
                }
            }
        };

        public int LatestSupportedVersion => this.questionnaireContentVersions.Last().Version;

        public bool IsClientVersionSupported(int clientVersion)
            => (clientVersion >= OldestQuestionnaireContentVersion && this.LatestSupportedVersion >= clientVersion);

        public IEnumerable<string> GetListOfNewFeaturesForClient(QuestionnaireDocument questionnaire, int clientVersion)
        {
            for (int nextClientVersion = clientVersion + 1; nextClientVersion <= this.LatestSupportedVersion; nextClientVersion++)
            {
                var questionnaireVersion = this.questionnaireContentVersions.FirstOrDefault(
                    contentVersion => contentVersion.Version == nextClientVersion);

                if(questionnaireVersion == null) continue;

                foreach (var newFeature in questionnaireVersion.NewFeatures)
                {
                    if (newFeature.HasQuestionnaire(questionnaire))
                        yield return newFeature.Description;
                }
            }
        }

        public int GetQuestionnaireContentVersion(QuestionnaireDocument questionnaireDocument)
        {
            foreach (var questionnaireContentVersion in this.questionnaireContentVersions.OrderByDescending(contentVersion => contentVersion.Version))
            {
                if (questionnaireContentVersion.NewFeatures.Any(newFeature => newFeature.HasQuestionnaire(questionnaireDocument)))
                    return questionnaireContentVersion.Version;
            }

            return OldestQuestionnaireContentVersion;
        }
    }
}
