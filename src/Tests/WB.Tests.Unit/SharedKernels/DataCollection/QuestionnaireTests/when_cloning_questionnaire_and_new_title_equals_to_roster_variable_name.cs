using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Implementation.Aggregates;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.QuestionnaireTests
{
    internal class when_cloning_questionnaire_and_new_title_equals_to_roster_variable_name : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            QuestionnaireDocument questionnaireDocument = Create.Entity.QuestionnaireDocument(children: new IComposite[]
            {
                Create.Entity.Roster(variable: rosterVariableName),
            });

            var plainQuestionnaireRepository = Mock.Of<IPlainQuestionnaireRepository>(_
                => _.GetQuestionnaireDocument(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version) == questionnaireDocument);

            IFileSystemAccessor fileSystemAccessor = Mock.Of<IFileSystemAccessor>();
            Mock.Get(fileSystemAccessor)
                .Setup(_ => _.MakeValidFileName(Moq.It.IsAny<string>()))
                .Returns<string>(filename => filename);

            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage
                = Setup.PlainStorageAccessorWithOneEntity<QuestionnaireBrowseItem>(
                    id: questionnaireIdentity.ToString(), entity: Create.Entity.QuestionnaireBrowseItem());

            questionnaire = Create.Other.DataCollectionQuestionnaire(
                questionnaireBrowseItemStorage: questionnaireBrowseItemStorage,
                plainQuestionnaireRepository: plainQuestionnaireRepository,
                fileSystemAccessor: fileSystemAccessor);
        };

        Because of = () =>
            questionnaireException = Catch.Only<QuestionnaireException>(() =>
                questionnaire.CloneQuestionnaire(Create.Command.CloneQuestionnaire(
                    questionnaireIdentity: questionnaireIdentity, newTitle: rosterVariableName)));

        It should_throw_QuestionnaireException_containing_specific_words = () =>
            questionnaireException.Message.ToLower().ToSeparateWords().ShouldContain("title", "roster");

        It should_throw_QuestionnaireException_containing_roster_variable_name = () =>
            questionnaireException.Message.ShouldContain(rosterVariableName);

        private static QuestionnaireException questionnaireException;
        private static Questionnaire questionnaire;
        private static QuestionnaireIdentity questionnaireIdentity
            = Create.Entity.QuestionnaireIdentity(Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"), 3);
        private static string rosterVariableName = "roster_var_name";
    }
}