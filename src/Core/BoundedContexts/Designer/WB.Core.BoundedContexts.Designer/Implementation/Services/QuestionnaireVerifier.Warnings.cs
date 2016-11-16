using System;
using System.Collections.Generic;
using System.Linq;
using CsQuery.ExtensionMethods;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.UI.Shared.Web.Extensions;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    internal partial class QuestionnaireVerifier : IQuestionnaireVerifier
    {
        private IEnumerable<Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>>> WarningsVerifiers => new[]
        {
            Warning(LargeNumberOfRosters, "WB0200", VerificationMessages.WB0200_LargeNumberOfRostersIsCreated),
            Warning<IGroup>(TooManyQuestionsInGroup, "WB0201", VerificationMessages.WB0201_LargeNumberOfQuestionsInGroup),
            Warning<IGroup>(EmptyGroup, "WB0202", VerificationMessages.WB0202_GroupWithoutQuestions),
            Warning<IGroup>(HasSingleQuestionInRoster, "WB0203", VerificationMessages.WB0203_RosterHasSingleQuestion),
            Warning<IGroup>(EmptyRoster, "WB0204", VerificationMessages.WB0204_EmptyRoster),
            Warning(TooManyQuestions, "WB0205", VerificationMessages.WB0205_TooManyQuestions),
            Warning(FewSectionsManyQuestions, "WB0206", VerificationMessages.WB0206_FewSectionsManyQuestions),
            Warning<IGroup>(FixedRosterContains3OrLessItems, "WB0207", VerificationMessages.WB0207_FixedRosterContains3OrLessItems),
            Warning(MoreThan50PercentQuestionsWithoutValidationConditions, "WB0208", VerificationMessages.WB0208_MoreThan50PercentsQuestionsWithoutValidationConditions),
            Warning<IComposite>(HasLongEnablementCondition, "WB0209", VerificationMessages.WB0209_LongEnablementCondition),
            Warning<IQuestion>(CategoricalQuestionHasALotOfOptions, "WB0210", VerificationMessages.WB0210_CategoricalQuestionHasManyOptions),
            Warning(HasNoGpsQuestions, "WB0211", VerificationMessages.WB0211_QuestionnaireHasNoGpsQuestion),
            Warning<IQuestion, ValidationCondition>(q => q.ValidationConditions, HasLongValidationCondition, "WB0212", index => string.Format(VerificationMessages.WB0212_LongValidationCondition, index)),
            Warning(AttachmentSizeIsMoreThan5Mb, "WB0213", VerificationMessages.WB0213_AttachmentSizeIsMoreThan5Mb),
            Warning(TotalAttachmentsSizeIsMoreThan50Mb, "WB0214", VerificationMessages.WB0214_TotalAttachmentsSizeIsMoreThan50Mb),
            Warning(UnusedAttachments, "WB0215", VerificationMessages.WB0215_UnusedAttachments),
            Warning(NoPrefilledQuestions, "WB0216", VerificationMessages.WB0216_NoPrefilledQuestions),
            Warning<IQuestion>(VariableLableMoreThan120Characters, "WB0217", VerificationMessages.WB0217_VariableLableMoreThan120Characters),
            WarningForCollection(ConsecutiveQuestionsWithIdenticalEnablementConditions, "WB0218", VerificationMessages.WB0218_ConsecutiveQuestionsWithIdenticalEnablementConditions),
            WarningForCollection(ConsecutiveUnconditionalSingleChoiceQuestionsWith2Options, "WB0219", VerificationMessages.WB0219_ConsecutiveUnconditionalSingleChoiceQuestionsWith2Options),
            Warning<IConditional>(RowIndexInMultiOptionBasedRoster, "WB0220", VerificationMessages.WB0220_RowIndexInMultiOptionBasedRoster),
            Warning<IValidatable>(RowIndexInMultiOptionBasedRoster, "WB0220", VerificationMessages.WB0220_RowIndexInMultiOptionBasedRoster),
            Warning(NoCurrentTimeQuestions, "WB0221", VerificationMessages.WB0221_NoCurrentTimeQuestions),
            Warning<SingleQuestion>(Prefilled, "WB0222", VerificationMessages.WB0222_SingleOptionPrefilled),
            Warning<IGroup>(NotSingleSectionWithLessThan5Questions, "WB0223", VerificationMessages.WB0223_SectionWithLessThan5Questions),
            Warning<IGroup>(TooManySubsectionsAtOneLevel, "WB0224", VerificationMessages.WB0224_TooManySubsectionsAtOneLevel),
            Warning<SingleQuestion>(ComboBoxWithLessThan10Elements, "WB0225", VerificationMessages.WB0225_ComboBoxWithLessThan10Elements),
            WarningForCollection(SameCascadingParentQuestion, "WB0226", VerificationMessages.WB0226_SameCascadingParentQuestion),
            Warning(NotShared, "WB0227", VerificationMessages.WB0227_NotShared),
            Warning<ICategoricalQuestion>(OmittedOptions, "WB0228", VerificationMessages.WB0228_OmittedOptions),
            Warning<IValidatable, IQuestionnaireEntity>(this.SupervisorQuestionInValidation, "WB0229", VerificationMessages.WB0229_SupervisorQuestionInValidation),
            Warning<ICategoricalQuestion>(NonconsecutiveCascadings, "WB0230", VerificationMessages.WB0230_NonconsecutiveCascadings),

            this.Warning_ValidationConditionRefersToAFutureQuestion_WB0250,
            this.Warning_EnablementConditionRefersToAFutureQuestion_WB0251,
            Warning(TooFewVariableLabelsAreDefined, "WB0253", VerificationMessages.WB0253_TooFewVariableLabelsAreDefined),
            Warning<IQuestion>(UseFunctionIsValidEmailToValidateEmailAddress, "WB0254", VerificationMessages.WB0254_UseFunctionIsValidEmailToValidateEmailAddress),
            Warning<IQuestion>(QuestionIsTooShort, "WB0255", VerificationMessages.WB0255_QuestionIsTooShort),
            Warning<GpsCoordinateQuestion>(Any, "WB0264", VerificationMessages.WB0264_GpsQuestion),
            Warning(MoreThan30PercentQuestionsAreText, "WB0265", VerificationMessages.WB0265_MoreThan30PercentQuestionsAreText),
            WarningForCollection(SameTitle, "WB0266", VerificationMessages.WB0266_SameTitle),
            Warning<QRBarcodeQuestion>(Any, "WB0267", VerificationMessages.WB0267_QRBarcodeQuestion),
        };

        private static bool NonconsecutiveCascadings(ICategoricalQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
            => question.GetPrevious() != GetCascadeFromQuestion(question, questionnaire);

        private static IQuestion GetCascadeFromQuestion(ICategoricalQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
            => question.CascadeFromQuestionId != null
                ? questionnaire.Questionnaire.Questionnaire.Find<IQuestion>(question.CascadeFromQuestionId.Value)
                : null;

        private EntityVerificationResult<IQuestionnaireEntity> SupervisorQuestionInValidation(IValidatable validatable, MultiLanguageQuestionnaireDocument questionnaire)
        {
            var supervisorQuestions = this
                .GetReferencedQuestions(validatable, questionnaire)
                .Where(question => question.QuestionScope == QuestionScope.Supervisor)
                .ToList();

            return supervisorQuestions.Count == 0
                ? EntityVerificationResult.NoProblems()
                : EntityVerificationResult.Problems(validatable, supervisorQuestions);
        }

        private static bool OmittedOptions(ICategoricalQuestion question)
        {
            int[] existingOptions = (question.Answers ?? Enumerable.Empty<Answer>())
                .Select(option => option.AnswerValue.ToIntOrNull())
                .Where(value => value.HasValue)
                .Select(value => value.Value)
                .OrderBy(x => x)
                .Distinct()
                .ToArray();

            if (existingOptions.Length == 0)
                return false;

            var minOption = existingOptions.Aggregate(Math.Min);
            var maxOption = existingOptions.Aggregate(Math.Max);

            int[] expectedOptions = Enumerable.Range(minOption, count: maxOption - minOption).ToArray();

            int[] missingOptions = expectedOptions.Except(existingOptions).OrderBy(x => x).ToArray();

            int firstIndex = 0;
            int lastIndex = missingOptions.Length - 1;
            for (int optionIndex = firstIndex; optionIndex <= lastIndex; optionIndex++)
            {
                var isStandaloneFromPrevious = optionIndex == firstIndex || missingOptions[optionIndex] - 1 != missingOptions[optionIndex - 1];
                var isStandaloneFromNext = optionIndex == lastIndex || missingOptions[optionIndex] + 1 != missingOptions[optionIndex + 1];
                bool isStandaloneMissingOption = isStandaloneFromPrevious && isStandaloneFromNext;

                if (isStandaloneMissingOption)
                    return true;
            }

            return false;
        }

        private static bool NotShared(MultiLanguageQuestionnaireDocument questionnaire)
            => !questionnaire.SharedPersons.Any();

        private static bool ComboBoxWithLessThan10Elements(SingleQuestion question)
            => (question.IsFilteredCombobox ?? false)
            && question.Answers.Count < 10;

        private static bool TooManySubsectionsAtOneLevel(IGroup group)
            => group.Children.Count(Subsection) >= 10;

        private static bool Subsection(IComposite entity)
            => entity is IGroup
            && !IsSection(entity)
            && !IsRoster(entity);

        private static bool NotSingleSectionWithLessThan5Questions(IGroup group)
            => IsSection(group)
            && group.GetParent().Children.Count > 1
            && group.GetDescendants().Count(Question) < 5;

        private static bool Question(IQuestionnaireEntity entity) => entity is IQuestion;
        private static bool Prefilled(IQuestion question) => question.Featured;

        private static bool RowIndexInMultiOptionBasedRoster(IConditional entity, MultiLanguageQuestionnaireDocument questionnaire)
            => UsesRowIndex(entity)
            && IsInsideMultiOptionBasedRoster(entity, questionnaire);

        private static bool RowIndexInMultiOptionBasedRoster(IValidatable entity, MultiLanguageQuestionnaireDocument questionnaire)
            => UsesRowIndex(entity)
            && IsInsideMultiOptionBasedRoster(entity, questionnaire);

        private static bool UsesRowIndex(IConditional entity)
            => entity.ConditionExpression?.Contains("@rowindex") == true;

        private static bool UsesRowIndex(IValidatable entity)
            => entity.ValidationConditions.Any(condition => condition.Expression.Contains("@rowindex"));

        private static bool IsInsideMultiOptionBasedRoster(IQuestionnaireEntity entity, MultiLanguageQuestionnaireDocument questionnaire)
            => entity.UnwrapReferences(x => x.GetParent()).Any(parent => IsMultiOptionBasedRoster(parent, questionnaire));

        private static bool IsMultiOptionBasedRoster(IQuestionnaireEntity entity, MultiLanguageQuestionnaireDocument questionnaire)
        {
            var group = entity as IGroup;

            if (group == null)
                return false;

            if (!group.IsRoster)
                return false;

            if (group.RosterSizeQuestionId == null)
                return false;

            return questionnaire.Find<IMultyOptionsQuestion>(group.RosterSizeQuestionId.Value) != null;
        }

        private static bool VariableLableMoreThan120Characters(IQuestion question)
            => (question.VariableLabel?.Length ?? 0) > 120;

        private static bool QuestionIsTooShort(IQuestion question)
        {
            if (string.IsNullOrEmpty(question.QuestionText) || !AnsweredManually(question))
                return false;

            return question.QuestionText.Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length <= 2;
        }

        private static bool Any(IComposite entity) => true;

        private static bool UseFunctionIsValidEmailToValidateEmailAddress(IQuestion question)
        {
            if (question.QuestionType != QuestionType.Text || string.IsNullOrEmpty(question.QuestionText) || question.ValidationConditions.Count >0)
                return false;

            return question.QuestionText.IndexOf("email", StringComparison.InvariantCultureIgnoreCase) >= 0;
        }

        private static bool TooFewVariableLabelsAreDefined(MultiLanguageQuestionnaireDocument questionnaire)
        {
            var countOfAllQuestions = questionnaire.Find<IQuestion>(AnsweredManually).Count();
            var countOfQuestionsWithoutLabels =
                questionnaire.Find<IQuestion>(q => string.IsNullOrEmpty(q.VariableLabel) && AnsweredManually(q)).Count();

            return countOfQuestionsWithoutLabels > (countOfAllQuestions/2);
        }

        private IEnumerable<QuestionnaireVerificationMessage> Warning_EnablementConditionRefersToAFutureQuestion_WB0251(MultiLanguageQuestionnaireDocument questionnaire)
        {
            var result = new List<QuestionnaireVerificationMessage>();
            var questionnairePlainStructure = questionnaire.GetAllEntitiesIdAndTypePairsInQuestionnaireFlowOrder().Select(e => e.Id).ToArray();
            var enitiesWithConditions =
               questionnaire.Find<IComposite>(
                   c => !string.IsNullOrEmpty((c as IConditional)?.ConditionExpression));

            foreach (var enitiesWithCondition in enitiesWithConditions)
            {
                var entityIndex = questionnairePlainStructure.IndexOf(enitiesWithCondition.PublicKey);
                var conditionExpression = ((IConditional)enitiesWithCondition).ConditionExpression;

                var referencedQuestions = this.GetReferencedQuestions(conditionExpression, questionnaire);

                foreach (var referencedQuestion in referencedQuestions)
                {
                    var indexOfReferencedQuestion =
                        questionnairePlainStructure.IndexOf(referencedQuestion.PublicKey);

                    if (indexOfReferencedQuestion > entityIndex)
                    {
                        result.Add(QuestionnaireVerificationMessage.Warning("WB0251",
                            VerificationMessages.WB0251_EnablementConditionRefersToAFutureQuestion,
                            CreateReference(enitiesWithCondition),
                            CreateReference(referencedQuestion)));
                    }
                }
            }
            return result;
        }

        private static IEnumerable<QuestionnaireNodeReference[]> ConsecutiveUnconditionalSingleChoiceQuestionsWith2Options(MultiLanguageQuestionnaireDocument questionnaire)
            => questionnaire
                .Find<SingleQuestion>()
                .Where(IsUnconditionalWith2Options)
                .GroupBy(question => new
                {
                    FirstConsecutiveUnconditionalSingleOptionQuestionWith2Options = question.UnwrapReferences(GetPreviousUnconditionalSingleOptionQuestionWith2Options).Last(),
                })
                .Where(grouping => grouping.Count() >= 3)
                .Select(grouping => grouping.Select(question => CreateReference(question)).ToArray());

        private static SingleQuestion GetPreviousUnconditionalSingleOptionQuestionWith2Options(SingleQuestion question)
        {
            var previousQuestion = question.GetPrevious() as SingleQuestion;

            return previousQuestion != null && IsUnconditionalWith2Options(previousQuestion) 
                ? previousQuestion
                : null;
        }

        private static bool IsUnconditionalWith2Options(SingleQuestion question)
            => string.IsNullOrWhiteSpace(question.ConditionExpression)
            && question.Answers.Count == 2;

        private static IEnumerable<QuestionnaireNodeReference[]> ConsecutiveQuestionsWithIdenticalEnablementConditions(MultiLanguageQuestionnaireDocument questionnaire)
            => questionnaire
                .Find<IQuestion>()
                .Where(question => !string.IsNullOrWhiteSpace(question.ConditionExpression))
                .GroupBy(question => new
                {
                    Enablement = question.ConditionExpression,
                    FirstConsecutiveQuestionWithSameEnablement = question.UnwrapReferences(GetPreviousQuestionWithSameEnablement).Last(),
                })
                .Where(grouping => grouping.Count() >= 3)
                .Select(grouping => grouping.Select(question => CreateReference(question)).ToArray());

        private static IQuestion GetPreviousQuestionWithSameEnablement(IQuestion question)
        {
            var previousQuestion = question.GetPrevious() as IQuestion;

            return previousQuestion?.ConditionExpression == question.ConditionExpression
                ? previousQuestion
                : null;
        }

        private static IEnumerable<QuestionnaireNodeReference[]> SameTitle(MultiLanguageQuestionnaireDocument questionnaire)
            => questionnaire
                .Find<IQuestion>()
                .GroupBy(question => question.QuestionText)
                .Where(grouping => grouping.Count() > 1)
                .Select(grouping => grouping.Select(question => CreateReference(question)).ToArray());

        private static IEnumerable<QuestionnaireNodeReference[]> SameCascadingParentQuestion(MultiLanguageQuestionnaireDocument questionnaire)
            => questionnaire
                .Find<SingleQuestion>(question => question.CascadeFromQuestionId.HasValue)
                .GroupBy(question => question.CascadeFromQuestionId)
                .Where(grouping => grouping.Count() > 1)
                .Select(grouping => grouping.Select(question => CreateReference(question)).ToArray());

        private IEnumerable<QuestionnaireVerificationMessage> Warning_ValidationConditionRefersToAFutureQuestion_WB0250(
            MultiLanguageQuestionnaireDocument questionnaire)
        {
            var result=new List<QuestionnaireVerificationMessage>();
            var questionnairePlainStructure = questionnaire.GetAllEntitiesIdAndTypePairsInQuestionnaireFlowOrder().Select(e=>e.Id).ToArray();

            var enitiesWithValidations =
                questionnaire.Find<IComposite>(
                    c => c is IValidatable && ((IValidatable) c).ValidationConditions.Count > 0);

            foreach (var enitiesWithValidation in enitiesWithValidations)
            {
                var entityIndex = questionnairePlainStructure.IndexOf(enitiesWithValidation.PublicKey);
                var validationIndex = 1;
                var validationConditions = ((IValidatable) enitiesWithValidation).ValidationConditions;
                foreach (var validationCondition in validationConditions)
                {
                    var referencedQuestions = this.GetReferencedQuestions(validationCondition, questionnaire);
                    foreach (var referencedQuestion in referencedQuestions)
                    {
                        var indexOfReferencedQuestion =
                            questionnairePlainStructure.IndexOf(referencedQuestion.PublicKey);

                        if (indexOfReferencedQuestion > entityIndex)
                        {
                            result.Add(QuestionnaireVerificationMessage.Warning("WB0250",
                                string.Format(VerificationMessages.WB0250_ValidationConditionRefersToAFutureQuestion, validationIndex),
                                CreateReference(enitiesWithValidation, validationIndex),
                                CreateReference(referencedQuestion)));
                        }
                    }
                    validationIndex++;
                }
            }
            return result;
        }

        private static bool AttachmentSizeIsMoreThan5Mb(AttachmentSize attachmentSize, MultiLanguageQuestionnaireDocument questionnaire) 
            => attachmentSize.Size > 5*1024*1024;

        private static bool UnusedAttachments(Attachment attachment, MultiLanguageQuestionnaireDocument questionnaire)
            => !questionnaire
                .Find<IStaticText>(t => t.AttachmentName == attachment.Name)
                .Any();

        private bool TotalAttachmentsSizeIsMoreThan50Mb(MultiLanguageQuestionnaireDocument questionnaire)
            => this.attachmentService
                .GetAttachmentSizesByQuestionnaire(questionnaire.PublicKey)
                .Sum(x => x.Size) > 50*1024*1024;

        private static bool HasLongValidationCondition(ValidationCondition condition)
            => !string.IsNullOrEmpty(condition.Expression) && condition.Expression.Length > 500;

        private static bool HasNoGpsQuestions(MultiLanguageQuestionnaireDocument questionnaire)
            => !questionnaire.Has<IQuestion>(q => q.QuestionType == QuestionType.GpsCoordinates);

        private static bool NoPrefilledQuestions(MultiLanguageQuestionnaireDocument questionnaire)
            => !questionnaire.Has<IQuestion>(q => q.Featured);

        private static bool NoCurrentTimeQuestions(MultiLanguageQuestionnaireDocument questionnaire)
            => !questionnaire.Has<DateTimeQuestion>(q => q.IsTimestamp);

        private static bool CategoricalQuestionHasALotOfOptions(IQuestion question)
            => question.QuestionType == QuestionType.SingleOption
            && !question.IsFilteredCombobox.GetValueOrDefault(false)
            && !question.CascadeFromQuestionId.HasValue
            && question.Answers.Count > 30;

        private bool HasLongEnablementCondition(IComposite groupOrQuestion)
        {
            var customEnablementCondition = GetCustomEnablementCondition(groupOrQuestion);

            if (string.IsNullOrEmpty(customEnablementCondition))
                return false;

            var exceeded = customEnablementCondition.Length > 500;
            return exceeded;
        }

        private static bool LargeNumberOfRosters(MultiLanguageQuestionnaireDocument questionnaire)
            => questionnaire.Find<IGroup>(q => q.IsRoster).Count() > 20;

        private static bool TooManyQuestions(MultiLanguageQuestionnaireDocument questionnaire)
            => questionnaire.Find<IQuestion>().Count() > 1000;

        private static bool FewSectionsManyQuestions(MultiLanguageQuestionnaireDocument questionnaire)
            => questionnaire.Find<IQuestion>().Count() > 100
            && questionnaire.Find<IGroup>(IsSection).Count() < 3;

        private static bool MoreThan30PercentQuestionsAreText(MultiLanguageQuestionnaireDocument questionnaire)
            => questionnaire.Find<TextQuestion>().Count(AnsweredManually) > 0.3 * questionnaire.Find<IQuestion>().Count(AnsweredManually);

        private static bool MoreThan50PercentQuestionsWithoutValidationConditions(MultiLanguageQuestionnaireDocument questionnaire)
            => questionnaire.Find<IQuestion>().Where(NoValidation).Count(AnsweredManually) > 0.5 * questionnaire.Find<IQuestion>().Count(AnsweredManually);

        private static bool HasSingleQuestionInRoster(IGroup rosterGroup)
            => rosterGroup.IsRoster
            && rosterGroup.Children.OfType<IQuestion>().Count() == 1;

        private static bool AnsweredManually(IQuestion question) => 
            !question.Featured && question.QuestionScope != QuestionScope.Headquarter && question.QuestionScope != QuestionScope.Hidden;

        private static bool TooManyQuestionsInGroup(IGroup group)
            => group.Children.OfType<IQuestion>().Count() > 200;

        private static bool EmptyGroup(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
            => !group.IsRoster
            && !group.Children.Any() 
            && questionnaire.Find<IGroup>().Count() > 1;

        private static bool FixedRosterContains3OrLessItems(IGroup group)
            => IsFixedRoster(group)
            && group.FixedRosterTitles.Length <= 3;

        private static bool EmptyRoster(IGroup group)
            => group.IsRoster
            && !group.Children.Any();

        private static bool IsSection(IQuestionnaireEntity entity) => entity.GetParent().GetParent() == null;

        private static bool IsFixedRoster(IGroup group) => group.IsRoster && (group.FixedRosterTitles?.Any() ?? false);

        private static bool NoValidation(IQuestion question) => !question.ValidationConditions.Any();

        private static bool IsNumeric(IQuestion question)
            => question.QuestionType == QuestionType.Numeric;

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Warning<TEntity, TSubEntity>(
            Func<TEntity, IEnumerable<TSubEntity>> getSubEnitites, Func<TSubEntity, bool> hasError, string code, Func<int, string> getMessageBySubEntityIndex)
            where TEntity : class, IComposite
        {
            return Warning(getSubEnitites, (entity, subEntity, questionnaire) => hasError(subEntity), code, getMessageBySubEntityIndex);
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Warning<TEntity, TSubEntity>(
            Func<TEntity, IEnumerable<TSubEntity>> getSubEnitites, Func<TEntity, TSubEntity, MultiLanguageQuestionnaireDocument, bool> hasError, string code, Func<int, string> getMessageBySubEntityIndex)
            where TEntity : class, IComposite
        {
            return questionnaire =>
                questionnaire
                    .Find<TEntity>(entity => true)
                    .SelectMany(entity => getSubEnitites(entity).Select((subEntity, index) => new { Entity = entity, SubEntity = subEntity, Index = index }))
                    .Where(descriptor => hasError(descriptor.Entity, descriptor.SubEntity, questionnaire))
                    .Select(descriptor => QuestionnaireVerificationMessage.Warning(code, getMessageBySubEntityIndex(descriptor.Index + 1), CreateReference(descriptor.Entity, descriptor.Index)));
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Warning<TEntity>(
            Func<TEntity, MultiLanguageQuestionnaireDocument, bool> hasError, string code, string message)
            where TEntity : class, IQuestionnaireEntity
        {
            return questionnaire =>
               questionnaire
                   .Find<TEntity>(x => hasError(x, questionnaire))
                   .Select(entity => QuestionnaireVerificationMessage.Warning(code, message, CreateReference(entity)));
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Warning<TEntity, TReferencedEntity>(
            Func<TEntity, MultiLanguageQuestionnaireDocument, EntityVerificationResult<TReferencedEntity>> verifyEntity, string code, string message)
            where TEntity : class, IQuestionnaireEntity
            where TReferencedEntity : class, IQuestionnaireEntity
        {
            return questionnaire =>
                from entity in questionnaire.Find<TEntity>(_ => true)
                let verificationResult = verifyEntity(entity, questionnaire)
                where verificationResult.HasErrors
                select QuestionnaireVerificationMessage.Warning(code, message, verificationResult.ReferencedEntities.Select(CreateReference).ToArray());
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Warning(
            Func<Attachment, MultiLanguageQuestionnaireDocument, bool> hasError, string code, string message)
        {
            return questionnaire =>
               questionnaire
                   .Attachments
                   .Where(x => hasError(x, questionnaire))
                   .Select(entity => QuestionnaireVerificationMessage.Warning(code, message, QuestionnaireNodeReference.CreateForAttachment(entity.AttachmentId)));
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> WarningForCollection(
            Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireNodeReference[]>> getReferences, string code, string message)
        {
            return questionnaire
                => getReferences(questionnaire)
                    .Select(references => QuestionnaireVerificationMessage.Warning(code, message, references));
        }

        private Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Warning(
            Func<AttachmentSize, MultiLanguageQuestionnaireDocument, bool> hasError, string code, string message)
        {
            return questionnaire =>
                   this.attachmentService.GetAttachmentSizesByQuestionnaire(questionnaire.PublicKey)
                   .Where(x => hasError(x, questionnaire))
                   .Select(entity => QuestionnaireVerificationMessage.Warning(code, message, QuestionnaireNodeReference.CreateForAttachment(entity.AttachmentId)));
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Warning<TEntity>(
           Func<TEntity, bool> hasError, string code, string message)
           where TEntity : class, IComposite
        {
            return questionnaire =>
                questionnaire
                    .Find<TEntity>(hasError)
                    .Select(entity => QuestionnaireVerificationMessage.Warning(code, message, CreateReference(entity)));
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Warning(
            Func<MultiLanguageQuestionnaireDocument, bool> hasError, string code, string message)
        {
            return questionnaire =>
                hasError(questionnaire)
                    ? new[] { QuestionnaireVerificationMessage.Warning(code, message) }
                    : Enumerable.Empty<QuestionnaireVerificationMessage>();
        }

        private IEnumerable<IQuestion> GetReferencedQuestions(string expression, MultiLanguageQuestionnaireDocument questionnaire)
            => this
                .GetIdentifiersUsedInExpression(expression, questionnaire)
                .Select(identifier => questionnaire.FirstOrDefault<IQuestion>(q => q.StataExportCaption == identifier))
                .Where(referencedQuestion => referencedQuestion != null);

        private IEnumerable<IQuestion> GetReferencedQuestions(ValidationCondition validationCondition, MultiLanguageQuestionnaireDocument questionnaire)
            => this.GetReferencedQuestions(validationCondition.Expression, questionnaire);

        private IEnumerable<IQuestion> GetReferencedQuestions(IValidatable validatable, MultiLanguageQuestionnaireDocument questionnaire)
            => validatable
                .ValidationConditions
                .SelectMany(condition => this.GetReferencedQuestions(condition, questionnaire))
                .Distinct();
    }
}
