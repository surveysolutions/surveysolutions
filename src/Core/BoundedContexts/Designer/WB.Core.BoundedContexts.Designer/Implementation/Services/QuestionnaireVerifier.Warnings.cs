using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

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
        };

        private static bool LargeNumberOfRosters(ReadOnlyQuestionnaireDocument questionnaire)
            => questionnaire.Find<IGroup>(q => q.IsRoster).Count() > 20;

        private static bool HasSingleQuestionInRoster(IGroup rosterGroup)
            => rosterGroup.IsRoster
            && rosterGroup.Children.OfType<IQuestion>().Count() == 1;

        private static bool TooManyQuestionsInGroup(IGroup group)
            => group.Children.OfType<IQuestion>().Count() > 200;

        private static bool GroupWithoutQuestions(IGroup group)
            => !group.IsRoster
            && !group.Children.OfType<IQuestion>().Any();

        private static bool EmptyRoster(IGroup group)
            => group.IsRoster
            && !group.Children.Any();

        private static Func<ReadOnlyQuestionnaireDocument, VerificationState, IEnumerable<QuestionnaireVerificationMessage>> Warning<TEntity, TSubEntity>(
            Func<TEntity, IEnumerable<TSubEntity>> getSubEnitites, Func<TSubEntity, bool> hasError, string code, Func<int, string> getMessageBySubEntityIndex)
            where TEntity : class, IComposite
        {
            return Verifier(getSubEnitites, (subEntity, state) => hasError(subEntity), code, getMessageBySubEntityIndex);
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
