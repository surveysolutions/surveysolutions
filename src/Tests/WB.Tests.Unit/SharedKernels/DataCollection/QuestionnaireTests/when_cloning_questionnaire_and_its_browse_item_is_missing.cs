using System;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Aggregates;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.QuestionnaireTests
{
    internal class when_cloning_questionnaire_and_its_browse_item_is_missing : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            var questionnaireBrowseItemStorage = Mock.Of<IPlainStorageAccessor<QuestionnaireBrowseItem>>(_
                => _.GetById(questionnaireIdentity.ToString()) == null as QuestionnaireBrowseItem);

            questionnaire = Create.DataCollectionQuestionnaire(
                questionnaireBrowseItemStorage: questionnaireBrowseItemStorage);
        };

        Because of = () =>
            questionnaireException = Catch.Only<QuestionnaireException>(() =>
                questionnaire.CloneQuestionnaire(Create.Command.CloneQuestionnaire(
                    questionnaireIdentity: questionnaireIdentity)));

        It should_throw_QuestionnaireException_containing_specific_words = () =>
            questionnaireException.Message.ToLower().ToSeparateWords().ShouldContain("questionnaire", "absent", "repository");

        private static QuestionnaireException questionnaireException;
        private static Questionnaire questionnaire;
        private static Guid questionnaireId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        private static long questionnaireVersion = 3;
        private static QuestionnaireIdentity questionnaireIdentity = Create.QuestionnaireIdentity(questionnaireId, questionnaireVersion);
    }
}