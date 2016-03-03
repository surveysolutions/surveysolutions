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
        private const int MaxRecommendedAmountOfRosters = 20;

        private IEnumerable<QuestionnaireVerificationMessage> VerifyAmountOfRosters(ReadOnlyQuestionnaireDocument document, VerificationState state)
        {
            var rosters = document.Find<IGroup>(q => q.IsRoster).ToArray();

            if (rosters.Length <= MaxRecommendedAmountOfRosters)
                return Enumerable.Empty<QuestionnaireVerificationMessage>();

            return new[]
            {
                QuestionnaireVerificationMessage.Warning("WB0200", VerificationMessages.WB0200_LargeNumberOfRostersIsCreated)
            };
        }

        private bool HasSingleQuestionInRoster(IGroup rosterGroup)
        {
            return rosterGroup.IsRoster && rosterGroup.Children.OfType<IQuestion>().Count() == 1;
        }

        private bool TooManyQuestionsInGroup(IGroup group)
        {
            return group.Children.OfType<IQuestion>().Count() > 200;
        }


        private bool GroupWithoutQuestions(IGroup group)
        {
            return !group.Children.OfType<IQuestion>().Any();
        }

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
    }
}
