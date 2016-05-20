using System;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Aggregates;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.QuestionnaireTests
{
    internal class when_cloning_questionnaire_and_its_browse_item_is_missing
    {
        Establish context = () =>
        {
            var questionnaireBrowseItemStorage = Setup.PlainStorageAccessorWithOneEntity<QuestionnaireBrowseItem>(
                id: questionnaireIdentity.ToString(), entity: null);

            questionnaire = Create.Other.DataCollectionQuestionnaire(
                questionnaireBrowseItemStorage: questionnaireBrowseItemStorage);
        };

        Because of = () =>
            questionnaireException = Catch.Only<QuestionnaireException>(() =>
                questionnaire.CloneQuestionnaire(Create.Command.CloneQuestionnaire(
                    questionnaireIdentity: questionnaireIdentity)));

        It should_throw_QuestionnaireException_containing_specific_words = () =>
            questionnaireException.Message.ToLower().ToSeparateWords().ShouldContain("questionnaire", "absent", "repository");

        It should_throw_QuestionnaireException_containing_questionnaire_id = () =>
            questionnaireException.Message.ShouldContain(questionnaireIdentity.QuestionnaireId.FormatGuid());

        It should_throw_QuestionnaireException_containing_questionnaire_version = () =>
            questionnaireException.Message.ShouldContain(questionnaireIdentity.Version.ToString());

        private static QuestionnaireException questionnaireException;
        private static Questionnaire questionnaire;
        private static QuestionnaireIdentity questionnaireIdentity
            = Create.Other.QuestionnaireIdentity(Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"), 3);
    }
}