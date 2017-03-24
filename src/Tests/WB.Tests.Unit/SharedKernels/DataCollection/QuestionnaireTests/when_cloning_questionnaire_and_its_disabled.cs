using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.Implementation.Aggregates;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.QuestionnaireTests
{
    internal class when_cloning_questionnaire_and_its_disabled
    {
        Establish context = () =>
        {
            var questionnaireBrowseItemStorage = Setup.PlainStorageAccessorWithOneEntity<QuestionnaireBrowseItem>(
                id: questionnaireIdentity.ToString(), entity: Create.Entity.QuestionnaireBrowseItem(disabled: true));

            questionnaire = Create.AggregateRoot.Questionnaire(
                questionnaireBrowseItemStorage: questionnaireBrowseItemStorage);
        };

        Because of = () =>
            questionnaireException = Catch.Only<QuestionnaireException>(() =>
                questionnaire.CloneQuestionnaire(Create.Command.CloneQuestionnaire(
                    questionnaireIdentity: questionnaireIdentity)));

        It should_throw_QuestionnaireException_containing_specific_words = () =>
            questionnaireException.Message.ToLower().ToSeparateWords().ShouldContain("questionnaire", "disabled");

        It should_throw_QuestionnaireException_containing_questionnaire_id = () =>
            questionnaireException.Message.ShouldContain(questionnaireIdentity.QuestionnaireId.FormatGuid());

        It should_throw_QuestionnaireException_containing_questionnaire_version = () =>
            questionnaireException.Message.ShouldContain(questionnaireIdentity.Version.ToString());

        private static QuestionnaireException questionnaireException;
        private static Questionnaire questionnaire;
        private static QuestionnaireIdentity questionnaireIdentity
            = Create.Entity.QuestionnaireIdentity(Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"), 3);
    }
}