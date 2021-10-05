using System;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Implementation.Aggregates;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.QuestionnaireTests
{
    internal class when_cloning_questionnaire_and_new_title_consists_only_of_spaces : QuestionnaireTestsContext
    {
        [NUnit.Framework.Test] public void should_throw_QuestionnaireException_containing_specific_words () {
            
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage
                = SetUp.PlainStorageAccessorWithOneEntity<QuestionnaireBrowseItem>(
                    id: questionnaireIdentity.ToString(), entity: Create.Entity.QuestionnaireBrowseItem());

            IQuestionnaireStorage questionnaireStorage =
                SetUp.QuestionnaireRepositoryWithOneQuestionnaire(
                    Create.Entity.QuestionnaireDocument(questionnaireIdentity.QuestionnaireId));
            
            questionnaire = Create.AggregateRoot.Questionnaire(questionnaireStorage: questionnaireStorage,
                questionnaireBrowseItemStorage: questionnaireBrowseItemStorage);
            
            var questionnaireException = Assert.Throws<QuestionnaireException>(() =>
                questionnaire.CloneQuestionnaire(Create.Command.CloneQuestionnaire(
                    questionnaireIdentity: questionnaireIdentity, newTitle: "   ")));
            questionnaireException.Message.ToLower().ToSeparateWords().Should().Contain("title", "empty");
        }

        private static Questionnaire questionnaire;
        private static QuestionnaireIdentity questionnaireIdentity
            = Create.Entity.QuestionnaireIdentity(Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"), 3);
    }
}
