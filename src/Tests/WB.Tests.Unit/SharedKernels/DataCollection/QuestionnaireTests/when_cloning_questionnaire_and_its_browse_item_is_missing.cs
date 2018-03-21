using System;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Implementation.Aggregates;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.DataCollection.QuestionnaireTests
{
    internal class when_cloning_questionnaire_and_its_browse_item_is_missing
    {
        [NUnit.Framework.Test] public void should_throw_QuestionnaireException_containing_questionnaire_version () {
            var questionnaireBrowseItemStorage = Setup.PlainStorageAccessorWithOneEntity<QuestionnaireBrowseItem>(
                id: questionnaireIdentity.ToString(), entity: null);

            questionnaire = Create.AggregateRoot.Questionnaire(
                questionnaireBrowseItemStorage: questionnaireBrowseItemStorage);

            var questionnaireException = Assert.Throws<QuestionnaireException>(() =>
                questionnaire.CloneQuestionnaire(Create.Command.CloneQuestionnaire(
                    questionnaireIdentity: questionnaireIdentity)));

            questionnaireException.Message.ToLower().ToSeparateWords().Should().Contain("questionnaire", "absent", "repository");
            questionnaireException.Message.Should().Contain(questionnaireIdentity.Version.ToString());
            questionnaireException.Message.Should().Contain(questionnaireIdentity.QuestionnaireId.FormatGuid());
        }

        private static Questionnaire questionnaire;
        private static QuestionnaireIdentity questionnaireIdentity
            = Create.Entity.QuestionnaireIdentity(Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"), 3);
    }
}
