using System;
using System.Linq;
using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.Implementation.Aggregates;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.QuestionnaireTests
{
    internal class when_cloning_questionnaire_and_new_title_is_501_symbol_long : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            longTitle = Enumerable.Range(1, 501).Select(_ => "x").Aggregate((s1, s2) => s1 + s2);

            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage
                = Setup.PlainStorageAccessorWithOneEntity<QuestionnaireBrowseItem>(
                    id: questionnaireIdentity.ToString(), entity: Create.Entity.QuestionnaireBrowseItem());

            questionnaire = Create.AggregateRoot.Questionnaire(
                questionnaireBrowseItemStorage: questionnaireBrowseItemStorage);
        };

        Because of = () =>
            questionnaireException = Catch.Only<QuestionnaireException>(() =>
                questionnaire.CloneQuestionnaire(Create.Command.CloneQuestionnaire(
                    questionnaireIdentity: questionnaireIdentity, newTitle: longTitle)));

        It should_throw_QuestionnaireException_containing_specific_words = () =>
            questionnaireException.Message.ToLower().ToSeparateWords().ShouldContain("title", "more", "500");

        private static QuestionnaireException questionnaireException;
        private static Questionnaire questionnaire;
        private static QuestionnaireIdentity questionnaireIdentity
            = Create.Entity.QuestionnaireIdentity(Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"), 3);
        private static string longTitle;
    }
}