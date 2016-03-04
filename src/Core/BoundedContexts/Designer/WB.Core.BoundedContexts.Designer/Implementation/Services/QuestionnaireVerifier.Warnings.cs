using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    internal partial class QuestionnaireVerifier : IQuestionnaireVerifier
    {
        private IEnumerable<Func<ReadOnlyQuestionnaireDocument, VerificationState, IEnumerable<QuestionnaireVerificationMessage>>> WarningsVerifiers => new[]
        {
            Warning(LargeNumberOfRosters, "WB0200", VerificationMessages.WB0200_LargeNumberOfRostersIsCreated),
            Warning<IGroup>(TooManyQuestionsInGroup, "WB0201", VerificationMessages.WB0201_LargeNumberOfQuestionsInGroup),
            Warning<IGroup>(GroupWithoutQuestions, "WB0202", VerificationMessages.WB0202_GroupWithoutQuestions),
            Warning<IGroup>(HasSingleQuestionInRoster, "WB0203", VerificationMessages.WB0203_RosterHasSingleQuestion),
            Warning<IGroup>(EmptyRoster, "WB0204", VerificationMessages.WB0204_EmptyRoster),
            Warning(TooManyQuestions, "WB0205", VerificationMessages.WB0205_TooManyQuestions),
            Warning(FewSectionsManyQuestions, "WB0206", VerificationMessages.WB0206_FewSectionsManyQuestions),
            Warning<IGroup>(FixedRosterContains3OrLessItems, "WB0207", VerificationMessages.WB0207_FixedRosterContains3OrLessItems),
            Warning(MoreThanHalfNumericQuestionsWithoutValidationConditions, "WB0208", VerificationMessages.WB0208_MoreThan50PercentsNumericQuestionsWithoutValidationConditions),
            Warning<IQuestion>(HasLongEnablementCondition, "WB0209", VerificationMessages.WB0209_LongEnablementCondition),
            Warning<IQuestion>(CategoricalQuestionHasALotOfOptions, "WB0210", VerificationMessages.WB0210_CategoricalQuestionHasManyOptions),
            Warning(HasNoGpsQuestions, "WB0211", VerificationMessages.WB0211_QuestionnaireHasNoGpsQuestion),

            Warning<IQuestion, ValidationCondition>(question => question.ValidationConditions, HasLongValidationCondition, "WB0212", index => string.Format(VerificationMessages.WB0212_LongValidationCondition, index)),
        };

        private static bool HasLongValidationCondition(ValidationCondition condition)
        {
            return !string.IsNullOrEmpty(condition.Expression) && condition.Expression.Length > 200;
        }

        private static bool HasNoGpsQuestions(ReadOnlyQuestionnaireDocument questionnaire)
            => !questionnaire.Find<IQuestion>(q => q.QuestionType == QuestionType.GpsCoordinates).Any();

        private static bool CategoricalQuestionHasALotOfOptions(IQuestion question)
        {
            return question.QuestionType == QuestionType.SingleOption && question.Answers.Count > 30;
        }

        private static bool HasLongEnablementCondition(IQuestion question)
        {
            return !string.IsNullOrEmpty(question.ConditionExpression) && question.ConditionExpression.Length > 200;
        }

        private static bool LargeNumberOfRosters(ReadOnlyQuestionnaireDocument questionnaire)
            => questionnaire.Find<IGroup>(q => q.IsRoster).Count() > 20;

        private static bool TooManyQuestions(ReadOnlyQuestionnaireDocument questionnaire)
            => questionnaire.Find<IQuestion>().Count() > 1000;

        private static bool FewSectionsManyQuestions(ReadOnlyQuestionnaireDocument questionnaire)
            => questionnaire.Find<IQuestion>().Count() > 100
            && questionnaire.Find<IGroup>(IsNotChapterOrRoot).Count() < 3;

        private static bool MoreThanHalfNumericQuestionsWithoutValidationConditions(ReadOnlyQuestionnaireDocument questionnaire)
            => questionnaire.Find<IQuestion>(IsNumericWithoutValidation).Count() > 0.5 * questionnaire.Find<IQuestion>().Count();

        private static bool HasSingleQuestionInRoster(IGroup rosterGroup)
            => rosterGroup.IsRoster
            && rosterGroup.Children.OfType<IQuestion>().Count() == 1;

        private static bool TooManyQuestionsInGroup(IGroup group)
            => group.Children.OfType<IQuestion>().Count() > 200;

        private static bool GroupWithoutQuestions(IGroup group)
            => !group.IsRoster
            && !group.Children.OfType<IQuestion>().Any();

        private static bool FixedRosterContains3OrLessItems(IGroup group)
            => IsFixedRoster(group)
            && group.FixedRosterTitles.Length <= 3;

        private static bool EmptyRoster(IGroup group)
            => group.IsRoster
            && !group.Children.Any();

        private static bool IsNotChapterOrRoot(IGroup group) => group.GetParent().GetParent() != null;

        private static bool IsFixedRoster(IGroup group) => group.IsRoster && (group.FixedRosterTitles?.Any() ?? false);

        private static bool IsNumericWithoutValidation(IQuestion question)
            => question.QuestionType == QuestionType.Numeric
            && !question.ValidationConditions.Any();

        private static IEnumerable<IComposite> GetDescendants(IGroup group) => group.Children.TreeToEnumerable(_ => _.Children);

        private static Func<ReadOnlyQuestionnaireDocument, VerificationState, IEnumerable<QuestionnaireVerificationMessage>> Warning<TEntity, TSubEntity>(
            Func<TEntity, IEnumerable<TSubEntity>> getSubEnitites, Func<TSubEntity, bool> hasError, string code, Func<int, string> getMessageBySubEntityIndex)
            where TEntity : class, IComposite
        {
            return Warning(getSubEnitites, (subEntity, state) => hasError(subEntity), code, getMessageBySubEntityIndex);
        }

        private static Func<ReadOnlyQuestionnaireDocument, VerificationState, IEnumerable<QuestionnaireVerificationMessage>> Warning<TEntity, TSubEntity>(
          Func<TEntity, IEnumerable<TSubEntity>> getSubEnitites, Func<TSubEntity, VerificationState, bool> hasError, string code, Func<int, string> getMessageBySubEntityIndex)
          where TEntity : class, IComposite
        {
            return Warning(getSubEnitites, (entity, subEntity, questionnaire, state) => hasError(subEntity, state), code, getMessageBySubEntityIndex);
        }

        private static Func<ReadOnlyQuestionnaireDocument, VerificationState, IEnumerable<QuestionnaireVerificationMessage>> Warning<TEntity, TSubEntity>(
            Func<TEntity, IEnumerable<TSubEntity>> getSubEnitites, Func<TEntity, TSubEntity, ReadOnlyQuestionnaireDocument, VerificationState, bool> hasError, string code, Func<int, string> getMessageBySubEntityIndex)
            where TEntity : class, IComposite
        {
            return (questionnaire, state) =>
                questionnaire
                    .Find<TEntity>(entity => true)
                    .SelectMany(entity => getSubEnitites(entity).Select((subEntity, index) => new { Entity = entity, SubEntity = subEntity, Index = index }))
                    .Where(descriptor => hasError(descriptor.Entity, descriptor.SubEntity, questionnaire, state))
                    .Select(descriptor => QuestionnaireVerificationMessage.Warning(code, getMessageBySubEntityIndex(descriptor.Index + 1), CreateReference(descriptor.Entity)));
        }

        private static Func<ReadOnlyQuestionnaireDocument, VerificationState, IEnumerable<QuestionnaireVerificationMessage>> Warning<TEntity>(
           Func<TEntity, bool> hasError, string code, string message)
           where TEntity : class, IComposite
        {
            return (questionnaire, state) =>
                questionnaire
                    .Find<TEntity>(hasError)
                    .Select(entity => QuestionnaireVerificationMessage.Warning(code, message, CreateReference(entity)));
        }

        private static Func<ReadOnlyQuestionnaireDocument, VerificationState, IEnumerable<QuestionnaireVerificationMessage>> Warning(
            Func<ReadOnlyQuestionnaireDocument, bool> hasError, string code, string message)
        {
            return (questionnaire, state) =>
                hasError(questionnaire)
                    ? new[] { QuestionnaireVerificationMessage.Warning(code, message) }
                    : Enumerable.Empty<QuestionnaireVerificationMessage>();
        }


   
    }
}
