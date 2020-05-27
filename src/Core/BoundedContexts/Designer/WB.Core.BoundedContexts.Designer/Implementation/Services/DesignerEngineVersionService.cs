using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    internal class DesignerEngineVersionService : IDesignerEngineVersionService
    {
        private readonly IAttachmentService attachmentService;

        public DesignerEngineVersionService(IAttachmentService attachmentService)
        {
            this.attachmentService = attachmentService;
        }

        private const int OldestQuestionnaireContentVersion = 16;

        private class QuestionnaireContentVersion
        {
            public int Version { get; set; }
            public IEnumerable<QuestionnaireFeature> NewFeatures { get; set; } = new List<QuestionnaireFeature>(); 
        }

        private class QuestionnaireFeature
        {
            public QuestionnaireFeature(string description, Func<QuestionnaireDocument, bool> hasQuestionnaire)
            {
                Description = description;
                HasQuestionnaire = hasQuestionnaire;
            }

            public string Description { get; set; }
            public Func<QuestionnaireDocument, bool> HasQuestionnaire { get; set; }
        }

        private List<QuestionnaireContentVersion> questionnaireContentVersions => new List<QuestionnaireContentVersion>
        {
            new QuestionnaireContentVersion
            {
                Version = 17, 
                NewFeatures = new []
                {
                    new QuestionnaireFeature
                    (
                          hasQuestionnaire : questionnaire =>  questionnaire.Translations.Any(),
                          description :"Multi-language questionnaire"
                    )
                }
            },
             new QuestionnaireContentVersion
            {
                Version = 18, 
                NewFeatures = new []
                {
                    new QuestionnaireFeature
                    (
                          hasQuestionnaire : questionnaire => questionnaire.Find<AbstractQuestion>(q => q.LinkedToQuestionId.HasValue && 
                                questionnaire.FirstOrDefault<AbstractQuestion>(x => x.PublicKey == q.LinkedToQuestionId)?.QuestionType == QuestionType.TextList).Any(),
                          description : "Question linked to List question"
                    )
                }
            },
            new QuestionnaireContentVersion
            {
                Version = 19,
                NewFeatures = new []
                {
                    new QuestionnaireFeature
                    (
                        hasQuestionnaire : questionnaire => 
                            questionnaire.Find<IConditional>(q => !string.IsNullOrEmpty(q.ConditionExpression) 
                                && q.ConditionExpression.Contains("rowindex")).Any()
                          ||
                            questionnaire.Find<IValidatable>(q => q.ValidationConditions.Count() == 1 
                                &&!string.IsNullOrEmpty(q.ValidationConditions.First().Expression) 
                                && q.ValidationConditions.First().Expression.Contains("rowindex")).Any(),
                        description : "Usage of @rowindex in expressions"
                    )
                }
            },
            new QuestionnaireContentVersion
            {
                Version = 20,
                NewFeatures = new []
                {
                    new QuestionnaireFeature
                    (
                        hasQuestionnaire : questionnaire => questionnaire.Find<AbstractQuestion>(q => q.QuestionType == QuestionType.Area).Any(),
                        description : "Area Question"
                    )
                }
            },
            new QuestionnaireContentVersion
            {
                Version = 21,
                NewFeatures = new []
                {
                    new QuestionnaireFeature
                    (
                        hasQuestionnaire : questionnaire => questionnaire.Find<AbstractQuestion>(q => q.QuestionType == QuestionType.Audio).Any(),
                        description : "Audio Question"
                    )
                }
            },
            new QuestionnaireContentVersion
            {
                Version = 22,
                NewFeatures = new []
                {
                    new QuestionnaireFeature
                    (
                        hasQuestionnaire : questionnaire => questionnaire.Find<INumericQuestion>(q => q.Answers?.Any() ?? false).Any(),
                        description : "Numeric question with special values"
                    ),
                    new QuestionnaireFeature
                    (
                        hasQuestionnaire : questionnaire => questionnaire.Find<IValidatable>(q => q.ValidationConditions.Any(x => x.Severity == ValidationSeverity.Warning)).Any(),
                        description : "Validation Warnings"
                    ),
                    new QuestionnaireFeature
                    (
                        hasQuestionnaire : questionnaire => questionnaire.Find<IMultimediaQuestion>(q => q.IsSignature).Any(),
                        description : "Signature Question"
                    )
                }
            },
            new QuestionnaireContentVersion
            {
                Version = 23,
                NewFeatures = new []
                {
                    new QuestionnaireFeature
                    (
                        hasQuestionnaire : questionnaire => true,
                        description : "New set of functions and geography question in conditions"
                    )
                }
            },
            new QuestionnaireContentVersion
            {
                Version = 24, 
                NewFeatures = new []
                {
                    new QuestionnaireFeature
                    (
                        hasQuestionnaire : questionnaire => questionnaire.Attachments.Any(a => IsNonImageAttachment(a.ContentId)),
                        description : "New types of attachments"
                    )
                }
            },
            new QuestionnaireContentVersion
            {
                Version = 25,
                NewFeatures = new []
                {
                    new QuestionnaireFeature
                    (
                        hasQuestionnaire : questionnaire => questionnaire.Find<IGroup>(q => !q.IsRoster && !String.IsNullOrWhiteSpace(q.VariableName)).Any(),
                        description : "Section with not empty variable name"
                    )
                }
            },
            new QuestionnaireContentVersion
            {
                Version = 26, 
                NewFeatures = new []
                {
                    new QuestionnaireFeature
                    (
                        hasQuestionnaire : questionnaire => questionnaire.Find<IMultyOptionsQuestion>(q => q.IsFilteredCombobox == true ).Any(),
                        description : "Multi option question with combobox kind"
                    ),
                    new QuestionnaireFeature
                    (
                        hasQuestionnaire : questionnaire => questionnaire.Find<SingleQuestion>(q => q.ShowAsList == true ).Any(),
                        description : "Cascading question with show as list option"
                    )
                }
            },
            new QuestionnaireContentVersion
            {
                Version = 27, // old versions for history and could be removed later
                NewFeatures = new []
                {
                    new QuestionnaireFeature
                    (
                        hasQuestionnaire : questionnaire =>
                        {
                            foreach (var composite in questionnaire.Find<IComposite>())
                            {
                                var title = composite.GetTitle();
                                const string selfSubstitution = "%self%";
                                if (title != null && title.Contains(selfSubstitution))
                                {
                                    return true;
                                }

                                if (composite is IValidatable validatable)
                                {
                                    foreach (var validationMessage in validatable.ValidationConditions.Select(x =>
                                        x.Message))
                                    {
                                        if (validationMessage != null && validationMessage.Contains(selfSubstitution))
                                        {
                                            return true;
                                        }
                                    }
                                }

                                var instructions = (composite as IQuestion)?.Instructions;

                                if (instructions != null && instructions.Contains(selfSubstitution))
                                {
                                    return true;
                                }
                            }
                            return false;
                        },
                        description : "%self% as substituted text"
                    ),
                    new QuestionnaireFeature
                    (
                        hasQuestionnaire : questionnaire => questionnaire.Find<IGroup>(q => q.DisplayMode == RosterDisplayMode.Table).Any(),
                        description : "Roster with table display mode"
                    ),
                    new QuestionnaireFeature
                    (
                        hasQuestionnaire : questionnaire => questionnaire.Find<IQuestion>(q => q.QuestionType == QuestionType.SingleOption
                                                                       && q.QuestionScope == QuestionScope.Supervisor 
                                                                       && q.CascadeFromQuestionId.HasValue).Any(),
                        description : "Cascading question with supervisor scope"
                    )
                }
            },
            new QuestionnaireContentVersion
            {
                Version = 28, 
                NewFeatures = new []
                {
                    new QuestionnaireFeature
                    (
                        hasQuestionnaire : questionnaire =>  questionnaire.Categories.Any(),
                        description : "Reusable categories"
                    ),
                    new QuestionnaireFeature
                    (
                        hasQuestionnaire : questionnaire => questionnaire.Find<IGroup>(q => q.DisplayMode == RosterDisplayMode.Matrix).Any(),
                        description : "Roster with Matrix display mode"
                    )
                }
            },
             new QuestionnaireContentVersion
            {
                Version = 29,
                NewFeatures = new []
                {
                    new QuestionnaireFeature
                    (
                        hasQuestionnaire : questionnaire =>  questionnaire.GetAllGroups().Any(x => x.CustomRosterTitle),
                        description : "Custom roster title"
                    ),
                    new QuestionnaireFeature
                    (
                        hasQuestionnaire : questionnaire =>  questionnaire.Find<ICategoricalQuestion>(question =>
                            !string.IsNullOrEmpty(question.Properties?.OptionsFilterExpression) 
                            && question.LinkedToQuestionId.HasValue 
                            && questionnaire.Find<IQuestion>(question.LinkedToQuestionId.Value)?.QuestionType == QuestionType.TextList).Any(),
                        description : "Option filter for categorical linked to list questions"
                    ),
                }
            },
             new QuestionnaireContentVersion
            {
                Version = ApiVersion.MaxQuestionnaireVersion,
                NewFeatures = new []
                {
                    new QuestionnaireFeature
                    {
                        HasQuestionnaire = questionnaire =>  questionnaire.IsCoverPageSupported,
                        Description = "New Cover page"
                    },
                }
            },
        };

        private bool IsNonImageAttachment(string contentId) =>
            !this.attachmentService.GetContent(contentId).IsImage();

        public int LatestSupportedVersion => this.questionnaireContentVersions.OrderBy(x => x.Version).Last().Version;

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
