using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Designer.Verifier
{
    public class AbstractVerifier
    {
        protected const int MaxMultiQuestionOptionsCount = 20;
        protected const int UnconditionalSingleChoiceQuestionOptionsCount = 2;
        protected const int MaxVariableLabelLength = 120;
        protected const int TextQuestionsLengthInPercents = 30;
        protected const int MaxAttachmentsSizeInMb = 50;
        protected const int MaxAttachmentSizeInMb = 5;
        protected const int MinValidationsInPercents = 50;
        protected const int MinFixedRosterItemsCount = 3;
        protected const int ManyQuestionsByFewSectionsCount = 100;
        protected const int MaxQuestionsCountInQuestionnaire = 1000;
        protected const int MaxQuestionsCountInSubSection = 200;
        protected const int MinCountOfDecimalPlaces = 1;
        protected const int MaxCountOfDecimalPlaces = 15;
        public const int MaxTitleLength = 500;
        protected const int MaxValidationMessageLength = 250;
        public const int MaxOptionLength = 250;
        protected const int MaxNestedSubsectionsCount = 10;
        protected const int MaxRosterSizeAnswer = 200;
        protected const int MaxQuestionnaireSizeInMb = 5;
        protected const int MinOptionsCount = 2;
        protected const int MaxNestedRostersCount = 3;

        protected const int MaxExpressionLength = 10000;
        protected const int MaxOptionsCountInCascadingQuestion = 15000;
        public const int MaxOptionsCountInFilteredComboboxQuestion = 15000;

        protected const int MaxOptionsCountInCategoricalOptionQuestion = 200;

        public const int DefaultVariableLengthLimit = 32;
        protected const int DefaultRestrictedVariableLengthLimit = 20;
        protected const int RosterVariableNameLimit = 28;

        protected const int MaxRosterPropagationLimit = 10000;
        protected const int QuestionnaireTotalEntitiesLimit = Constants.MaxTotalRosterPropagationLimit;
        protected const int MaxQuestionsCountInSection = 400;
        protected const int MaxUIEntitiesInPlainModeGroup = 20;
        protected const int MaxEntitiesInTableRoster = 10;
        protected const int MaxEntitiesInMatrixRoster = 1;

        public const string VariableRegularExpression = "^(?!.*[_]{2})[A-Za-z][_A-Za-z0-9]*(?<!_)$";
        public static readonly Regex VariableNameRegex = new Regex(VariableRegularExpression);

        protected static QuestionnaireEntityReference CreateReference(IQuestionnaireEntity entity)
        {
            return QuestionnaireEntityReference.CreateFrom(entity);
        }

        protected static QuestionnaireEntityReference CreateReference(IComposite entity, int? failedValidationIndex)
        {
            return new QuestionnaireEntityReference(
                entity is IGroup
                    ? QuestionnaireVerificationReferenceType.Group
                    : entity is IStaticText
                        ? QuestionnaireVerificationReferenceType.StaticText
                        : entity is IVariable
                            ? QuestionnaireVerificationReferenceType.Variable
                            : QuestionnaireVerificationReferenceType.Question,
                entity.PublicKey)
            {
                IndexOfEntityInProperty = failedValidationIndex
            };
        }

        protected static long CalculateRosterInstancesCountAndUpdateCache(IGroup roster, Dictionary<Guid, long> rosterPropagationCounts, MultiLanguageQuestionnaireDocument questionnaire)
        {
            long rosterCount;
            if (rosterPropagationCounts.TryGetValue(roster.PublicKey, out rosterCount))
                return rosterCount;

            long parentCountMultiplier = 1;
            var questionnaireItem = roster.GetParent();
            while (questionnaireItem != null)
            {
                if (questionnaire.Questionnaire.IsRoster(questionnaireItem))
                {
                    parentCountMultiplier = CalculateRosterInstancesCountAndUpdateCache((IGroup)questionnaireItem, rosterPropagationCounts, questionnaire);
                    break;
                }

                questionnaireItem = questionnaireItem.GetParent();
            }

            long rosterMaxPropagationCount = parentCountMultiplier * GetRosterMaxPropagationCount(roster, questionnaire);
            rosterPropagationCounts.Add(roster.PublicKey, rosterMaxPropagationCount);

            return rosterMaxPropagationCount;
        }

        protected bool IsVariableNameValid(string variableName)
        {
            if (string.IsNullOrEmpty(variableName))
                return true;

            if (variableName.Length > DefaultVariableLengthLimit)
                return false;

            return VariableNameRegex.IsMatch(variableName);
        }

        protected static int GetRosterMaxPropagationCount(IGroup roster, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (!questionnaire.Questionnaire.IsRoster(roster))
                return 0;

            if (questionnaire.Questionnaire.IsFixedRoster(roster))
            {
                return roster.FixedRosterTitles.Length;
            }

            int questionMaxAnswersCount = Constants.MaxRosterRowCount;
            if (roster.RosterSizeQuestionId == null)
                return questionMaxAnswersCount;

            var question = questionnaire.Questionnaire.GetEntityByIdOrNull(roster.RosterSizeQuestionId.Value) as IQuestion;
            switch (question)
            {
                case MultyOptionsQuestion multyOptionsQuestion:
                    questionMaxAnswersCount = multyOptionsQuestion.MaxAllowedAnswers ?? multyOptionsQuestion.Answers?.Count ?? 0;
                    break;
                case TextListQuestion textListTrigger when textListTrigger.MaxAnswerCount != null:
                    questionMaxAnswersCount = textListTrigger.MaxAnswerCount.Value;
                    break;
            }

            return questionMaxAnswersCount;
        }

        protected static bool IsSection(IQuestionnaireEntity entity) => entity.GetParent()?.GetParent() == null;

        protected static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Error<TEntity>(string code, Func<TEntity, MultiLanguageQuestionnaireDocument, bool> hasError, string message)
            where TEntity : class, IComposite
        {
            return questionnaire =>
                questionnaire
                    .Find<TEntity>(entity => hasError(entity, questionnaire))
                    .Select(entity => QuestionnaireVerificationMessage.Error(code, message, CreateReference(entity)));
        }
        
        protected static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Error<TEntity>(string code, Func<TEntity, MultiLanguageQuestionnaireDocument, IQuestionnaireEntity?> hasEntityWithError, string message)
            where TEntity : class, IComposite
        {
            return questionnaire =>
                questionnaire
                    .Questionnaire
                    .Find<TEntity>()
                    .Select(entity => hasEntityWithError(entity, questionnaire))
                    .Where(entity => entity != null)
                    .Select(entity => QuestionnaireVerificationMessage.Error(code, message, CreateReference(entity!)));
        }
        
        protected static QuestionnaireVerificationReferenceType GetReferenceTypeByItemTypeAndId(MultiLanguageQuestionnaireDocument questionnaire, Guid id, Type entityType)
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
    }
}
