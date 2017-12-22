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
                Version = 17, 
                NewFeatures = new []
                {
                    new QuestionnaireFeature
                    {
                          HasQuestionnaire = questionnaire =>  questionnaire.Translations.Any(),
                          Description = "Multilanguage questionnaire"
                    }
                }
            },
             new QuestionnaireContentVersion
            {
                Version = 18, 
                NewFeatures = new []
                {
                    new QuestionnaireFeature
                    {
                          HasQuestionnaire = questionnaire => questionnaire.Find<AbstractQuestion>(q => q.LinkedToQuestionId.HasValue && 
                                questionnaire.FirstOrDefault<AbstractQuestion>(x => x.PublicKey == q.LinkedToQuestionId)?.QuestionType == QuestionType.TextList).Any(),
                          Description = "Question linked to List question"
                    }
                }
            },
            new QuestionnaireContentVersion
            {
                Version = 19,
                NewFeatures = new []
                {
                    new QuestionnaireFeature
                    {
                          HasQuestionnaire = questionnaire => 
                            questionnaire.Find<IConditional>(q => !string.IsNullOrEmpty(q.ConditionExpression) 
                                && q.ConditionExpression.Contains("rowindex")).Any()
                          ||
                            questionnaire.Find<IValidatable>(q => q.ValidationConditions.Count() == 1 
                                &&!string.IsNullOrEmpty(q.ValidationConditions.First().Expression) 
                                && q.ValidationConditions.First().Expression.Contains("rowindex")).Any(),
                          Description = "Usage of @rowindex in expressions"
                    }
                }
            },
            new QuestionnaireContentVersion
            {
                Version = 20,
                NewFeatures = new []
                {
                    new QuestionnaireFeature
                    {
                          HasQuestionnaire = questionnaire => questionnaire.Find<AbstractQuestion>(q => q.QuestionType == QuestionType.Area).Any(),
                          Description = "New expression storage or contains Area Question"
                    }
                }
            },
            new QuestionnaireContentVersion()
            {
                Version = ApiVersion.MaxQuestionnaireVersion, /*When adding new version, it should be changed to previous value of ApiVersion.MaxQuestionnaireVersion*/
                NewFeatures = new []
                {
                    new QuestionnaireFeature
                    {
                        HasQuestionnaire = questionnaire => questionnaire.Find<AbstractQuestion>(q => q.QuestionType == QuestionType.Audio).Any(),
                        Description = "Contains Audio Question"
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
