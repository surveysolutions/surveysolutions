using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Verifier
{
    public class QuestionnaireVerifications : AbstractVerifier, IPartialVerifier
    {
        private readonly ISubstitutionService substitutionService;
        private readonly IKeywordsProvider keywordsProvider;
        private static readonly Regex QuestionnaireNameRegex = new Regex(@"^[\w, \-\(\)\\/]*$");

        public QuestionnaireVerifications(ISubstitutionService substitutionService, IKeywordsProvider keywordsProvider)
        {
            this.substitutionService = substitutionService;
            this.keywordsProvider = keywordsProvider;
        }

        private IEnumerable<Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>>> ErrorsVerifiers => new[]
        {
            Error("WB0001", NoQuestionsExist, VerificationMessages.WB0001_NoQuestions),
            Error("WB0097", QuestionnaireTitleHasInvalidCharacters, VerificationMessages.WB0097_QuestionnaireTitleHasInvalidCharacters),
            Error("WB0119", QuestionnaireTitleTooLong, string.Format(VerificationMessages.WB0119_QuestionnaireTitleTooLong, MaxTitleLength)),
            Error("WB0098", QuestionnaireHasSizeMoreThan5Mb, size => VerificationMessages.WB0098_QuestionnaireHasSizeMoreThan5MB.FormatString(size, MaxQuestionnaireSizeInMb)),
            Error("WB0261", QuestionnaireHasRostersPropagationsExededLimit, VerificationMessages.WB0261_RosterStructureTooExplosive),
            Error("WB0277", QuestionnaireTitleHasConsecutiveUnderscores, VerificationMessages.WB0277_QuestionnaireTitleCannotHaveConsecutiveUnderscore),
            Error<IComposite, int>("WB0121", VariableNameTooLong, length => string.Format(VerificationMessages.WB0121_VariableNameTooLong, length)),
            Error<IComposite>("WB0124", VariableNameEndWithUnderscore, VerificationMessages.WB0124_VariableNameEndWithUnderscore),
            Error<IComposite>("WB0125", VariableNameHasConsecutiveUnderscores, VerificationMessages.WB0125_VariableNameHasConsecutiveUnderscores),
            Critical<IComposite>("WB0067", VariableNameIsEmpty, string.Format(VerificationMessages.WB0067_VariableNameIsEmpty)),
            Critical<IComposite>("WB0058", VariableNameIsKeywords, VerificationMessages.WB0058_QuestionHasVariableNameReservedForServiceNeeds),
            Critical<IComposite>("WB0122", VariableNameHasSpecialCharacters, VerificationMessages.WB0122_VariableNameHasSpecialCharacters),
            Critical<IComposite>("WB0123", VariableNameStartWithDigitOrUnderscore, VerificationMessages.WB0123_VariableNameStartWithDigitOrUnderscore),
            
            ErrorsByQuestionnaireEntitiesShareSameInternalId,
            ErrorsBySubstitutions,
            ErrorsByInvalidQuestionnaireVariable,
            Critical_EntitiesWithDuplicateVariableName_WB0026,
            Warning(NotShared, "WB0227", VerificationMessages.WB0227_NotShared),
        };

        private static readonly IEnumerable<QuestionType> QuestionTypesValidToBeSubstitutionReferences = new[]
        {
            QuestionType.DateTime,
            QuestionType.Numeric,
            QuestionType.SingleOption,
            QuestionType.Text,
            QuestionType.QRBarcode
        };

        private static readonly QuestionType[] RestrictedVariableLengthQuestionTypes =
        {
            QuestionType.GpsCoordinates,
            QuestionType.MultyOption,
            QuestionType.TextList
        };

        private static IEnumerable<QuestionnaireVerificationMessage> Critical_EntitiesWithDuplicateVariableName_WB0026(
            MultiLanguageQuestionnaireDocument questionnaire)
        {
            var rosterVariableNameMappedOnRosters = questionnaire
                .Find<IGroup>(g => g.IsRoster && !string.IsNullOrEmpty(g.VariableName))
                .Select(r => new
                {
                    Name = r.VariableName,
                    Reference = QuestionnaireEntityReference.CreateForRoster(r.PublicKey)
                })
                .Union(questionnaire.Find<IQuestion>(q => true)
                    .Where(x => !string.IsNullOrEmpty(x.StataExportCaption))
                    .Select(r => new
                    {
                        Name = r.StataExportCaption,
                        Reference = CreateReference(r)
                    }))
                .Union(questionnaire.LookupTables.Where(x => !string.IsNullOrEmpty(x.Value.TableName))
                    .Select(r => new
                    {
                        Name = r.Value.TableName,
                        Reference = QuestionnaireEntityReference.CreateForLookupTable(r.Key)
                    }))
                .Union(questionnaire.Find<IVariable>(x => !string.IsNullOrEmpty(x.Name))
                    .Where(x => !string.IsNullOrEmpty(x.Name))
                    .Select(r => new
                    {
                        Name = r.Name,
                        Reference = CreateReference(r)
                    })
                ).Union(questionnaire.VariableName.ToEnumerable()
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Select(r => new
                    {
                        Name = r,
                        Reference = QuestionnaireEntityReference.CreateForQuestionnaire(questionnaire.PublicKey)
                    })
                ).ToList();


            return rosterVariableNameMappedOnRosters
                .GroupBy(s => s.Name, StringComparer.InvariantCultureIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => QuestionnaireVerificationMessage.Critical(
                    "WB0026",
                    VerificationMessages.WB0026_ItemsWithTheSameNamesFound,
                    group.Select(x => x.Reference).ToArray()));
        }
        
        private IEnumerable<QuestionnaireVerificationMessage> ErrorsByInvalidQuestionnaireVariable(MultiLanguageQuestionnaireDocument questionnaire)
        {
            var foundErrors = new List<QuestionnaireVerificationMessage>();

            var reference = QuestionnaireEntityReference.CreateForQuestionnaire(questionnaire.PublicKey);
            if (string.IsNullOrWhiteSpace(questionnaire.VariableName))
                foundErrors.Add(QuestionnaireVerificationMessage.Error("WB0067", VerificationMessages.WB0067_VariableNameIsEmpty, reference));

            if (VariableNameHasSpecialCharacters(questionnaire.Questionnaire.Questionnaire, questionnaire))
                foundErrors.Add(QuestionnaireVerificationMessage.Error("WB0122", VerificationMessages.WB0122_VariableNameHasSpecialCharacters, reference));

            if (VariableNameStartWithDigitOrUnderscore(questionnaire.Questionnaire.Questionnaire, questionnaire))
                foundErrors.Add(QuestionnaireVerificationMessage.Error("WB0123", VerificationMessages.WB0123_VariableNameStartWithDigitOrUnderscore, reference));

            if (VariableNameIsKeywords(questionnaire.Questionnaire.Questionnaire, questionnaire))
                foundErrors.Add(QuestionnaireVerificationMessage.Error("WB0058", VerificationMessages.WB0058_QuestionHasVariableNameReservedForServiceNeeds, reference));

            if (VariableNameEndWithUnderscore(questionnaire.Questionnaire.Questionnaire, questionnaire))
                foundErrors.Add(QuestionnaireVerificationMessage.Error("WB0124", VerificationMessages.WB0124_VariableNameEndWithUnderscore, reference));

            if (VariableNameHasConsecutiveUnderscores(questionnaire.Questionnaire.Questionnaire, questionnaire))
                foundErrors.Add(QuestionnaireVerificationMessage.Error("WB0125", VerificationMessages.WB0125_VariableNameHasConsecutiveUnderscores, reference));

            return foundErrors.Distinct(new QuestionnaireVerificationMessage.CodeAndReferencesAndTranslationComparer());
        }


        private static bool VariableNameIsEmpty(IComposite entity, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (entity is IGroup && !questionnaire.Questionnaire.IsRoster(entity))
                return false;
            if (entity is StaticText)
                return false;
            return string.IsNullOrWhiteSpace(entity.VariableName);
        }

        private static Tuple<bool, int> VariableNameTooLong(IComposite entity, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (string.IsNullOrWhiteSpace(entity.VariableName))
                return Tuple.Create(false, 0);

            int variableLengthLimit = DefaultVariableLengthLimit;
            if (entity is IQuestion)
            {
                variableLengthLimit = RestrictedVariableLengthQuestionTypes.Contains((entity as IQuestion).QuestionType)
                    ? DefaultRestrictedVariableLengthLimit
                    : DefaultVariableLengthLimit;
            }

            if (entity is IGroup group && group.IsRoster)
            {
                variableLengthLimit = RosterVariableNameLimit;
            }

            var result = (entity.VariableName?.Length ?? 0) > variableLengthLimit;
            return Tuple.Create(result, variableLengthLimit);
        }

        private static bool VariableNameHasSpecialCharacters(IComposite entity, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (string.IsNullOrWhiteSpace(entity.VariableName))
                return false;
            foreach (var c in entity.VariableName)
            {
                if (c != '_' && !Char.IsDigit(c) && !((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')))
                    return true;
            }
            return false;
        }

        private static bool VariableNameStartWithDigitOrUnderscore(IComposite entity, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (string.IsNullOrWhiteSpace(entity.VariableName))
                return false;
            var variable = entity.VariableName;
            return Char.IsDigit(variable[0]) || variable[0] == '_';
        }

        private static bool VariableNameEndWithUnderscore(IComposite entity, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (string.IsNullOrWhiteSpace(entity.VariableName))
                return false;
            var variable = entity.VariableName;
            return variable[variable.Length - 1] == '_';
        }

        private static bool VariableNameHasConsecutiveUnderscores(IComposite entity, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (string.IsNullOrWhiteSpace(entity.VariableName))
                return false;

            return entity.VariableName.Contains("__");
        }

        private static bool QuestionnaireTitleHasConsecutiveUnderscores(MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (string.IsNullOrWhiteSpace(questionnaire.Title))
                return false;

            return questionnaire.Title.Contains("__");
        }

        private bool VariableNameIsKeywords(IComposite entity, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (string.IsNullOrWhiteSpace(entity.VariableName))
                return false;
            return keywordsProvider.IsReservedKeyword(entity.VariableName);
        }

        private static bool NotShared(MultiLanguageQuestionnaireDocument questionnaire)
            => !questionnaire.SharedPersons.Any();

        private IEnumerable<QuestionnaireVerificationMessage> ErrorsBySubstitutions(
            MultiLanguageQuestionnaireDocument questionnaire)
        {
            var foundErrors = new List<QuestionnaireVerificationMessage>();

            var entitiesSupportingSubstitutions = questionnaire.FindWithTranslations<IComposite>(SupportsSubstitutions).ToList();

            foreach (var translatedEntity in entitiesSupportingSubstitutions)
            {
                foundErrors.AddRange(this.GetErrorsBySubstitutionsInEntityTitleOrInstructions(translatedEntity, translatedEntity.Entity.GetTitle(), questionnaire));
                
                if (translatedEntity.Entity is IValidatable entityAsValidatable)
                {
                    var validationConditions = entityAsValidatable.ValidationConditions;

                    for (int validationConditionIndex = 0; validationConditionIndex < validationConditions.Count; validationConditionIndex++)
                    {
                        var validationCondition = validationConditions[validationConditionIndex];
                        foundErrors.AddRange(this.GetErrorsBySubstitutionsInValidationCondition(translatedEntity, validationCondition.Message, validationConditionIndex, questionnaire));
                    }
                }

                if (translatedEntity.Entity is IQuestion entityAsQuestion)
                {
                    var variableLabel= entityAsQuestion.VariableLabel;
                    if (!string.IsNullOrWhiteSpace(variableLabel))
                      foundErrors.AddRange(GetErrorsBySubstitutionsInVariableLabel(translatedEntity, variableLabel, questionnaire));

                    if (!string.IsNullOrWhiteSpace(entityAsQuestion.Instructions))
                        foundErrors.AddRange(this.GetErrorsBySubstitutionsInEntityTitleOrInstructions(translatedEntity, entityAsQuestion.Instructions, questionnaire));
                }
            }

            return foundErrors.Distinct(new QuestionnaireVerificationMessage.CodeAndReferencesAndTranslationComparer());
        }

        private IEnumerable<QuestionnaireVerificationMessage> GetErrorsBySubstitutionsInVariableLabel(MultiLanguageQuestionnaireDocument.TranslatedEntity<IComposite> translatedEntity, string variableLabel, MultiLanguageQuestionnaireDocument questionnaire)
        {
            string[] substitutionReferences = this.substitutionService.GetAllSubstitutionVariableNames(variableLabel);

            if (substitutionReferences.Any())
            {
                return QuestionnaireVerificationMessage.Error("WB0008",
                    VerificationMessages.WB0008_SubstitutionsInVariableLableAreProhibited,
                    translatedEntity.TranslationName,
                    CreateReference(translatedEntity.Entity)).ToEnumerable();
            }
            return Enumerable.Empty<QuestionnaireVerificationMessage>();
        }

        private IEnumerable<QuestionnaireVerificationMessage> GetErrorsBySubstitutionsInValidationCondition(MultiLanguageQuestionnaireDocument.TranslatedEntity<IComposite> translatedEntity, string validationCondition, int validationConditionIndex, MultiLanguageQuestionnaireDocument questionnaire)
        {
            string[] substitutionReferences = this.substitutionService.GetAllSubstitutionVariableNames(validationCondition);

            if (!substitutionReferences.Any())
                return Enumerable.Empty<QuestionnaireVerificationMessage>();

            Guid[] vectorOfRosterSizeQuestionsForEntityWithSubstitution = questionnaire.Questionnaire.GetRosterScope(translatedEntity.Entity);

            IEnumerable<QuestionnaireVerificationMessage> entityErrors = substitutionReferences
                .Select(identifier => this.GetVerificationErrorsBySubstitutionReferenceOrNull(
                    translatedEntity, validationConditionIndex, identifier, vectorOfRosterSizeQuestionsForEntityWithSubstitution, questionnaire))
                .Where(errorOrNull => errorOrNull != null);

            return entityErrors;
        }

        private IEnumerable<QuestionnaireVerificationMessage> GetErrorsBySubstitutionsInEntityTitleOrInstructions(MultiLanguageQuestionnaireDocument.TranslatedEntity<IComposite> translatedEntity, string title, MultiLanguageQuestionnaireDocument questionnaire)
        {
            string[] substitutionReferences = this.substitutionService.GetAllSubstitutionVariableNames(title);

            if (!substitutionReferences.Any())
                return Enumerable.Empty<QuestionnaireVerificationMessage>();

            var question = translatedEntity.Entity as IQuestion;
            if (question != null && questionnaire.Questionnaire.IsPreFilledQuestion(question))
                return QuestionWithTitleSubstitutionCantBePrefilled(question).ToEnumerable();

            Guid[] vectorOfRosterSizeQuestionsForEntityWithSubstitution = questionnaire.Questionnaire.GetRosterScope(translatedEntity.Entity);

            IEnumerable<QuestionnaireVerificationMessage> entityErrors = substitutionReferences
                .Select(identifier => this.GetVerificationErrorsBySubstitutionReferenceOrNull(
                    translatedEntity, null, identifier, vectorOfRosterSizeQuestionsForEntityWithSubstitution, questionnaire))
                .Where(errorOrNull => errorOrNull != null);

            return entityErrors;
        }

        private QuestionnaireVerificationMessage GetVerificationErrorsBySubstitutionReferenceOrNull(
            MultiLanguageQuestionnaireDocument.TranslatedEntity<IComposite> traslatedEntityWithSubstitution,
            int? validationConditionIndex,
            string substitutionReference,
            RosterScope vectorOfRosterQuestionsByEntityWithSubstitutions,
            MultiLanguageQuestionnaireDocument questionnaire)
        {
            bool isTitleOrInstructions = validationConditionIndex == null;
            var referenceToEntityWithSubstitution = CreateReference(traslatedEntityWithSubstitution.Entity, validationConditionIndex);

            if (traslatedEntityWithSubstitution.Entity is IQuestion question && isTitleOrInstructions && substitutionReference == question.StataExportCaption)
            {
                return QuestionnaireVerificationMessage.Error("WB0016",
                    VerificationMessages.WB0016_QuestionWithTitleSubstitutionCantReferenceSelf,
                    traslatedEntityWithSubstitution.TranslationName,
                    referenceToEntityWithSubstitution);
            }

            if (substitutionReference == this.substitutionService.RosterTitleSubstitutionReference)
            {
                bool isCheckedEntityaRoster = traslatedEntityWithSubstitution.Entity is IGroup group && group.IsRoster; 
                if (vectorOfRosterQuestionsByEntityWithSubstitutions.Length == 0 || 
                         vectorOfRosterQuestionsByEntityWithSubstitutions.Length == 1 && isCheckedEntityaRoster)
                {
                    return QuestionnaireVerificationMessage.Error("WB0059",
                        VerificationMessages.WB0059_EntityUsesRostertitleSubstitutionAndNeedsToBePlacedInsideRoster,
                        traslatedEntityWithSubstitution.TranslationName,
                        referenceToEntityWithSubstitution);
                }
                return null;
            }

            var entityToSubstitute = GetEntityByVariable(substitutionReference, questionnaire);
            if (entityToSubstitute == null)
            {
                return QuestionnaireVerificationMessage.Error("WB0017",
                    VerificationMessages.WB0017_SubstitutionReferencesNotExistingQuestionOrVariable,
                    traslatedEntityWithSubstitution.TranslationName,
                    referenceToEntityWithSubstitution);
            }

            var referenceToEntityBeingSubstituted = CreateReference(entityToSubstitute);

            var isVariable = entityToSubstitute is IVariable;
            var isQuestion = entityToSubstitute is IQuestion;
            var isRoster = (entityToSubstitute as IGroup)?.IsRoster ?? false;
            var isNotVariableOrQuestionOrRoster = !(isVariable || isQuestion || isRoster);
            var isQuestionOfNotSupportedType = isQuestion && !QuestionTypesValidToBeSubstitutionReferences.Contains(((IQuestion)entityToSubstitute).QuestionType);
            if (isNotVariableOrQuestionOrRoster || isQuestionOfNotSupportedType)
            {
                return QuestionnaireVerificationMessage.Error("WB0018",
                    VerificationMessages.WB0018_SubstitutionReferencesUnsupportedEntity,
                    traslatedEntityWithSubstitution.TranslationName,
                    referenceToEntityWithSubstitution,
                    referenceToEntityBeingSubstituted);
            }

            var entityToSubstituteRosterScope = questionnaire.Questionnaire.GetRosterScope(entityToSubstitute);

            if (!entityToSubstituteRosterScope.IsSameOrParentScopeFor(vectorOfRosterQuestionsByEntityWithSubstitutions))
            {
                return QuestionnaireVerificationMessage.Error("WB0019",
                    VerificationMessages.WB0019_SubstitutionCantReferenceItemWithDeeperRosterLevel,
                    traslatedEntityWithSubstitution.TranslationName,
                    referenceToEntityWithSubstitution,
                    referenceToEntityBeingSubstituted);
            }

            return null;
        }

        private static IComposite GetEntityByVariable(string identifier, MultiLanguageQuestionnaireDocument questionnaire)
            => questionnaire.FirstOrDefault<IQuestion>(q => q.StataExportCaption == identifier) as IComposite
               ?? questionnaire.FirstOrDefault<IVariable>(v => v.Name == identifier) as IComposite
               ?? questionnaire.FirstOrDefault<IGroup>(g => g.VariableName == identifier) as IComposite;

        private static QuestionnaireVerificationMessage QuestionWithTitleSubstitutionCantBePrefilled(IQuestion questionsWithSubstitution)
            => QuestionnaireVerificationMessage.Error("WB0015",
                VerificationMessages.WB0015_QuestionWithTitleSubstitutionCantBePrefilled,
                CreateReference(questionsWithSubstitution));

        private static bool SupportsSubstitutions(IComposite entity)
            => entity is IQuestion
               || entity is IStaticText
               || entity is IGroup;

        private static IEnumerable<QuestionnaireVerificationMessage> ErrorsByQuestionnaireEntitiesShareSameInternalId(MultiLanguageQuestionnaireDocument questionnaire)
        {
            return questionnaire
                .GetAllEntitiesIdAndTypePairsInQuestionnaireFlowOrder()
                .GroupBy(x => x.Id)
                .Where(group => group.Count() > 1)
                .Select(group =>
                    QuestionnaireVerificationMessage.Critical(
                        "WB0102",
                        VerificationMessages.WB0102_QuestionnaireEntitiesShareSameInternalId,
                        group.Select(x => new QuestionnaireEntityReference(GetReferenceTypeByItemTypeAndId(questionnaire, x.Id, x.Type), x.Id)).ToArray()));
        }


        private static QuestionnaireVerificationReferenceType GetReferenceTypeByItemTypeAndId(MultiLanguageQuestionnaireDocument questionnaire, Guid id, Type entityType)
        {
            if (typeof(IQuestion).IsAssignableFrom(entityType))
                return QuestionnaireVerificationReferenceType.Question;

            if (entityType.IsAssignableFrom(typeof(StaticText)))
                return QuestionnaireVerificationReferenceType.StaticText;

            if (entityType.IsAssignableFrom(typeof(Variable)))
                return QuestionnaireVerificationReferenceType.Variable;

            var group = questionnaire.Find<IGroup>(id);

            return questionnaire.Questionnaire.IsRoster(group)
                ? QuestionnaireVerificationReferenceType.Roster
                : QuestionnaireVerificationReferenceType.Group;
        }

        private static bool QuestionnaireHasRostersPropagationsExededLimit(MultiLanguageQuestionnaireDocument questionnaire)
        {
            var rosters = questionnaire.Find<IGroup>(q => q.IsRoster).ToList();
            Dictionary<Guid, long> rosterPropagationCounts = new Dictionary<Guid, long>();
            foreach (var roster in rosters)
            {
                CalculateRosterInstancesCountAndUpdateCache(roster, rosterPropagationCounts, questionnaire);
            }

            return rosterPropagationCounts.Values.Sum(x => x) > MaxTotalRosterPropagationLimit;
        }

        private static Tuple<bool, decimal> QuestionnaireHasSizeMoreThan5Mb(MultiLanguageQuestionnaireDocument questionnaire)
        {
            var jsonQuestionnaire = JsonConvert.SerializeObject(questionnaire.Questionnaire, Formatting.None);
            var questionnaireByteCount = Encoding.UTF8.GetByteCount(jsonQuestionnaire);
            var isOversized = questionnaireByteCount > 5 * 1024 * 1024; // 5MB
            var questionnaireMegaByteCount = (decimal)questionnaireByteCount / 1024 / 1024;
            return new Tuple<bool, decimal>(isOversized, questionnaireMegaByteCount);
        }

        private static bool NoQuestionsExist(MultiLanguageQuestionnaireDocument questionnaire)
        {
            return !questionnaire.Find<IQuestion>(_ => true).Any();
        }

        private static bool QuestionnaireTitleTooLong(MultiLanguageQuestionnaireDocument questionnaire)
        {
            return (questionnaire.Title?.Length ?? 0) > MaxTitleLength;
        }

        private static bool QuestionnaireTitleHasInvalidCharacters(MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (string.IsNullOrWhiteSpace(questionnaire.Title))
            {
                return false;
            }

            return !QuestionnaireNameRegex.IsMatch(questionnaire.Title);
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Error<TArg>(string code, Func<MultiLanguageQuestionnaireDocument, Tuple<bool, TArg>> hasError, Func<TArg, string> messageBuilder)
        {
            return questionnaire =>
            {
                var errorCheckResult = hasError(questionnaire);
                return errorCheckResult.Item1
                    ? new[] { QuestionnaireVerificationMessage.Error(code, messageBuilder.Invoke(errorCheckResult.Item2)) }
                    : Enumerable.Empty<QuestionnaireVerificationMessage>();
            };
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Error<TEntity, TArg>(string code, Func<TEntity, MultiLanguageQuestionnaireDocument, Tuple<bool, TArg>> hasError, Func<TArg, string> messageBuilder)
            where TEntity : class, IComposite
        {
            IEnumerable<QuestionnaireVerificationMessage> LocalFunction(MultiLanguageQuestionnaireDocument questionnaire)
            {
                foreach (TEntity entity in questionnaire.Find<TEntity>())
                {
                    var hasErrorResult = hasError(entity, questionnaire);
                    if (hasErrorResult.Item1)
                    {
                        yield return QuestionnaireVerificationMessage.Error(code, messageBuilder.Invoke(hasErrorResult.Item2), CreateReference(entity));
                    }
                }
            }

            return LocalFunction;
        }
        
        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Error(string code, Func<MultiLanguageQuestionnaireDocument, bool> hasError, string message)
        {
            return questionnaire =>
                hasError(questionnaire)
                    ? new[] { QuestionnaireVerificationMessage.Error(code, message) }
                    : Enumerable.Empty<QuestionnaireVerificationMessage>();
        }


        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Warning(
            Func<MultiLanguageQuestionnaireDocument, bool> hasError, string code, string message)
        {
            return questionnaire =>
                hasError(questionnaire)
                    ? new[] { QuestionnaireVerificationMessage.Warning(code, message) }
                    : Enumerable.Empty<QuestionnaireVerificationMessage>();
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Error<TEntity>(string code, Func<TEntity, MultiLanguageQuestionnaireDocument, bool> hasError, string message)
            where TEntity : class, IComposite
        {
            return questionnaire =>
                questionnaire
                    .Find<TEntity>(entity => hasError(entity, questionnaire))
                    .Select(entity => QuestionnaireVerificationMessage.Error(code, message, CreateReference(entity)));
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Critical<TEntity>(
            string code, Func<TEntity, MultiLanguageQuestionnaireDocument, bool> hasError,  string message)
            where TEntity : class, IComposite
        {
            return questionnaire =>
                questionnaire
                    .Find<TEntity>(entity => hasError(entity, questionnaire))
                    .Select(entity => QuestionnaireVerificationMessage.Critical(code, message, CreateReference(entity)));
        }

        public IEnumerable<QuestionnaireVerificationMessage> Verify(MultiLanguageQuestionnaireDocument multiLanguageQuestionnaireDocument)
        {
            var verificationMessagesByQuestionnaire = new List<QuestionnaireVerificationMessage>();
            foreach (var verifier in ErrorsVerifiers)
            {
                verificationMessagesByQuestionnaire.AddRange(verifier.Invoke(multiLanguageQuestionnaireDocument));
            }
            return verificationMessagesByQuestionnaire;
        }
    }
}
