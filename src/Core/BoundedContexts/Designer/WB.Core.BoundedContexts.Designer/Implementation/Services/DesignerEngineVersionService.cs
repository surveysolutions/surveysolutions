using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    internal class DesignerEngineVersionService : IDesignerEngineVersionService
    {
        private const int OldestQuestionnaireContentVersion = 10;
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
                Version = 11,
                NewFeatures = new[]
                {
                    new QuestionnaireFeature
                    {
                        HasQuestionnaire = (questionnaire) => questionnaire.LookupTables.Count > 0,
                        Description = "Lookup tables"
                    },
                    new QuestionnaireFeature
                    {
                        HasQuestionnaire = (questionnaire) => questionnaire.Find<IMultyOptionsQuestion>(q => q.YesNoView).Any(),
                        Description = "Yes/No questions"
                    },
                    new QuestionnaireFeature
                    {
                        HasQuestionnaire = (questionnaire) => questionnaire.Find<IQuestion>(q => !string.IsNullOrEmpty(q.ConditionExpression) &&
                                                                                                 q.ConditionExpression.Contains("IsValidEmail")).Any() 
                                                              ||
                                                              questionnaire.Find<IQuestion>(q => q.ValidationConditions.Count() == 1 &&
                                                                                                 !string.IsNullOrEmpty(q.ValidationConditions.First().Expression) &&
                                                                                                 q.ValidationConditions.First().Expression.Contains("IsValidEmail")).Any(),
                        Description = "Expression uses IsValidEmail"
                    }
                }
            },
            new QuestionnaireContentVersion
            {
                Version = 12,
                NewFeatures = new[]
                {
                    new QuestionnaireFeature
                    {
                        HasQuestionnaire = (questionnaire) => questionnaire.Find<IQuestion>(q => q.LinkedToRosterId.HasValue).Any(),
                        Description = "Linked on roster title question"
                    },
                    new QuestionnaireFeature
                    {
                        HasQuestionnaire = (questionnaire) => questionnaire.Find<IQuestion>(q => q.ValidationConditions.Count() > 1).Any(),
                        Description = "Multiple validations"
                    }
                }
            },
            new QuestionnaireContentVersion
            {
                Version = 13,
                NewFeatures = new[]
                {
                    new QuestionnaireFeature
                    {
                        HasQuestionnaire = (questionnaire) => questionnaire.Find<IQuestion>(q => !string.IsNullOrEmpty(q.LinkedFilterExpression)).Any(),
                        Description = "Filtered linked questions"
                    },
                    new QuestionnaireFeature
                    {
                        HasQuestionnaire = (questionnaire) => questionnaire.Find<StaticText>(q => !string.IsNullOrWhiteSpace(q.AttachmentName)).Any(),
                        Description = "Attachments: Images in static texts"
                    }
                }
            },
            new QuestionnaireContentVersion
            {
                Version = 14,
                NewFeatures = new[]
                {
                    new QuestionnaireFeature
                    {
                        HasQuestionnaire = (questionnaire) => questionnaire.Find<StaticText>(x => x.ValidationConditions.Any() || !string.IsNullOrWhiteSpace(x.ConditionExpression)).Any(),
                        Description = "Static texts: enablement conditions and validations"
                    }
                }
            },
            new QuestionnaireContentVersion
            {
                Version = 15,
                NewFeatures = new[]
                {
                    new QuestionnaireFeature
                    {
                        HasQuestionnaire = (questionnaire) => questionnaire.Find<Variable>().Any(),
                        Description = "Variables"
                    }
                }
            },
            new QuestionnaireContentVersion
            {
                Version = 16 ,
                NewFeatures = new[]
                {
                    new QuestionnaireFeature
                    {
                          HasQuestionnaire = (questionnaire) =>  questionnaire.Find<IQuestion>(q => !string.IsNullOrEmpty(q.LinkedFilterExpression)&& q.LinkedFilterExpression.Contains("current")).Any(),
                          Description = "Filtered linked questions that uses @current variable"
                    },
                    new QuestionnaireFeature
                    {
                        HasQuestionnaire = (questionnaire) => questionnaire.Find<DateTimeQuestion>(dateTimeQuestion => dateTimeQuestion.IsTimestamp).Any(),
                        Description = "Current time questions"
                    },
                    new QuestionnaireFeature
                    {
                        HasQuestionnaire = (questionnaire) => questionnaire.Find<AbstractQuestion>(question => !string.IsNullOrWhiteSpace(question.Properties.OptionsFilterExpression)).Any(),
                        Description = "Filtered options for categorical questions"
                    }
                }
            },
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
                Version = ApiVersion.MaxQuestionnaireVersion, /*When will be added new version, it should be changed to previous value of ApiVersion.MaxQuestionnaireVersion*/
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
            }

             //@rowindex
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
