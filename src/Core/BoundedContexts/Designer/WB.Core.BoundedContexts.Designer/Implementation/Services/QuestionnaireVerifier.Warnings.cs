using System;
using System.Collections.Generic;
using System.Linq;
using CsQuery.ExtensionMethods;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Core.SharedKernels.SurveySolutions.Documents;

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
            Warning(MoreThanHalfNumericQuestionsWithoutValidationConditions, "WB0208", VerificationMessages.WB0208_MoreThan50PercentsNumericQuestionsWithoutValidationConditions),
            Warning<IComposite>(HasLongEnablementCondition, "WB0209", VerificationMessages.WB0209_LongEnablementCondition),
            Warning<IQuestion>(CategoricalQuestionHasALotOfOptions, "WB0210", VerificationMessages.WB0210_CategoricalQuestionHasManyOptions),
            Warning<IQuestion>(UseFunctionIsValidEmailToValidateEmailAddress, "WB0254", VerificationMessages.WB0254_UseFunctionIsValidEmailToValidateEmailAddress),
            Warning<IQuestion>(QuestionIsTooShort, "WB0255", VerificationMessages.WB0255_QuestionIsTooShort),

            Warning(HasNoGpsQuestions, "WB0211", VerificationMessages.WB0211_QuestionnaireHasNoGpsQuestion),

            Warning<IQuestion, ValidationCondition>(question => question.ValidationConditions, HasLongValidationCondition, "WB0212", index => string.Format(VerificationMessages.WB0212_LongValidationCondition, index)),

            Warning(TotalAttachmentsSizeIsMoreThan50Mb, "WB0214", VerificationMessages.WB0214_TotalAttachmentsSizeIsMoreThan50Mb),
            Warning(UnusedAttachments, "WB0215", VerificationMessages.WB0215_UnusedAttachments),
            Warning(AttachmentSizeIsMoreThan5Mb, "WB0213", VerificationMessages.WB0213_AttachmentSizeIsMoreThan5Mb),
            Warning(TooFewVariableLabelsAreDefined, "WB0253", VerificationMessages.WB0253_TooFewVariableLabelsAreDefined),

            Warning(ValidationConditionRefersToAFutureQuestion),
            Warning(EnablementConditionRefersToAFutureQuestion)
        };

        private bool QuestionIsTooShort(IQuestion question)
        {
            if (string.IsNullOrEmpty(question.QuestionText) || !AnsweredManually(question))
                return false;

            return question.QuestionText.Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length <= 2;
        }

        private bool UseFunctionIsValidEmailToValidateEmailAddress(IQuestion question)
        {
            if (question.QuestionType != QuestionType.Text || string.IsNullOrEmpty(question.QuestionText) || question.ValidationConditions.Count >0)
                return false;

            return question.QuestionText.IndexOf("email", StringComparison.InvariantCultureIgnoreCase) >= 0;
        }

        private bool TooFewVariableLabelsAreDefined(MultiLanguageQuestionnaireDocument questionnaire)
        {
            var countOfAllQuestions = questionnaire.Find<IQuestion>(AnsweredManually).Count();
            var countOfQuestionsWithoutLabels =
                questionnaire.Find<IQuestion>(q => string.IsNullOrEmpty(q.VariableLabel) && AnsweredManually(q)).Count();

            return countOfQuestionsWithoutLabels > (countOfAllQuestions/2);
        }

        private IEnumerable<QuestionnaireVerificationMessage> EnablementConditionRefersToAFutureQuestion(MultiLanguageQuestionnaireDocument questionnaire)
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

                var referencedQuestions = this.expressionProcessor
                    .GetIdentifiersUsedInExpression(macrosSubstitutionService.InlineMacros(conditionExpression,
                        questionnaire.Macros.Values))
                    .Select(
                        identifier => questionnaire.FirstOrDefault<IQuestion>(q => q.StataExportCaption == identifier))
                    .Where(referencedQuestion => referencedQuestion != null);

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

        private IEnumerable<QuestionnaireVerificationMessage> ValidationConditionRefersToAFutureQuestion(
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
                    var referencedQuestions = this.expressionProcessor
                        .GetIdentifiersUsedInExpression(macrosSubstitutionService.InlineMacros(validationCondition.Expression, questionnaire.Macros.Values))
                        .Select(identifier => questionnaire.FirstOrDefault<IQuestion>(q => q.StataExportCaption == identifier))
                        .Where(referencedQuestion => referencedQuestion != null);
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

        private bool AttachmentSizeIsMoreThan5Mb(AttachmentSize attachmentSize, MultiLanguageQuestionnaireDocument questionnaire) 
            => attachmentSize.Size > 5*1024*1024;

        private bool UnusedAttachments(Attachment attachment, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return !questionnaire
                .Find<IStaticText>(t => t.AttachmentName == attachment.Name)
                .Any();
        }

        private bool TotalAttachmentsSizeIsMoreThan50Mb(MultiLanguageQuestionnaireDocument questionnaire)
        {
            return this.attachmentService
                .GetAttachmentSizesByQuestionnaire(questionnaire.PublicKey)
                .Sum(x => x.Size) > 50*1024*1024; // 50 Mb;
        }

        private static bool HasLongValidationCondition(ValidationCondition condition)
        {
            return !string.IsNullOrEmpty(condition.Expression) && condition.Expression.Length > 500;
        }

        private static bool HasNoGpsQuestions(MultiLanguageQuestionnaireDocument questionnaire)
            => !questionnaire.Find<IQuestion>(q => q.QuestionType == QuestionType.GpsCoordinates).Any();

        private static bool CategoricalQuestionHasALotOfOptions(IQuestion question)
        {
            return question.QuestionType == QuestionType.SingleOption && 
                   !question.IsFilteredCombobox.GetValueOrDefault(false) && 
                   !question.CascadeFromQuestionId.HasValue &&
                   question.Answers.Count > 30;
        }

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

        private static bool MoreThanHalfNumericQuestionsWithoutValidationConditions(MultiLanguageQuestionnaireDocument questionnaire)
            => questionnaire.Find<IQuestion>().Count(x => IsNumericWithoutValidation(x) && AnsweredManually(x)) > 0.5 * questionnaire.Find<IQuestion>().Count(IsNumeric);

        private static bool HasSingleQuestionInRoster(IGroup rosterGroup)
            => rosterGroup.IsRoster
            && rosterGroup.Children.OfType<IQuestion>().Count() == 1;

        private static bool AnsweredManually(IQuestion question) => 
            !question.Featured && question.QuestionScope != QuestionScope.Headquarter && question.QuestionScope != QuestionScope.Hidden;

        private static bool TooManyQuestionsInGroup(IGroup group)
            => group.Children.OfType<IQuestion>().Count() > 200;

        private static bool EmptyGroup(IGroup group)
            => !group.IsRoster
            && !group.Children.Any();

        private static bool FixedRosterContains3OrLessItems(IGroup group)
            => IsFixedRoster(group)
            && group.FixedRosterTitles.Length <= 3;

        private static bool EmptyRoster(IGroup group)
            => group.IsRoster
            && !group.Children.Any();

        private static bool IsSection(IGroup group) => group.GetParent().GetParent() == null;

        private static bool IsFixedRoster(IGroup group) => group.IsRoster && (group.FixedRosterTitles?.Any() ?? false);

        private static bool IsNumericWithoutValidation(IQuestion question)
            => IsNumeric(question)
            && !question.ValidationConditions.Any();

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
            return (questionnaire) =>
                questionnaire
                    .Find<TEntity>(entity => true)
                    .SelectMany(entity => getSubEnitites(entity).Select((subEntity, index) => new { Entity = entity, SubEntity = subEntity, Index = index }))
                    .Where(descriptor => hasError(descriptor.Entity, descriptor.SubEntity, questionnaire))
                    .Select(descriptor => QuestionnaireVerificationMessage.Warning(code, getMessageBySubEntityIndex(descriptor.Index + 1), CreateReference(descriptor.Entity, descriptor.Index)));
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Warning<TEntity>(
            Func<TEntity, MultiLanguageQuestionnaireDocument, bool> hasError, string code, string message)
            where TEntity : class, IComposite
        {
            return (questionnaire) =>
               questionnaire
                   .Find<TEntity>(x => hasError(x, questionnaire))
                   .Select(entity => QuestionnaireVerificationMessage.Warning(code, message, CreateReference(entity)));
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Warning(
            Func<Attachment, MultiLanguageQuestionnaireDocument, bool> hasError, string code, string message)
        {
            return (questionnaire) =>
               questionnaire
                   .Attachments
                   .Where(x => hasError(x, questionnaire))
                   .Select(entity => QuestionnaireVerificationMessage.Warning(code, message, QuestionnaireVerificationReference.CreateForAttachment(entity.AttachmentId)));
        }


        private Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>>
            Warning(
            Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>>
                validationConditionRefersToAFutureQuestion)
        {
            return (questionnaire) => validationConditionRefersToAFutureQuestion(questionnaire);
        }

        private Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Warning(
            Func<AttachmentSize, MultiLanguageQuestionnaireDocument, bool> hasError, string code, string message)
        {
            return (questionnaire) =>
                   this.attachmentService.GetAttachmentSizesByQuestionnaire(questionnaire.PublicKey)
                   .Where(x => hasError(x, questionnaire))
                   .Select(entity => QuestionnaireVerificationMessage.Warning(code, message, QuestionnaireVerificationReference.CreateForAttachment(entity.AttachmentId)));
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Warning<TEntity>(
           Func<TEntity, bool> hasError, string code, string message)
           where TEntity : class, IComposite
        {
            return (questionnaire) =>
                questionnaire
                    .Find<TEntity>(hasError)
                    .Select(entity => QuestionnaireVerificationMessage.Warning(code, message, CreateReference(entity)));
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Warning(
            Func<MultiLanguageQuestionnaireDocument, bool> hasError, string code, string message)
        {
            return (questionnaire) =>
                hasError(questionnaire)
                    ? new[] { QuestionnaireVerificationMessage.Warning(code, message) }
                    : Enumerable.Empty<QuestionnaireVerificationMessage>();
        }
    }
}
