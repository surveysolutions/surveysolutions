using System;
using System.Linq;
using Main.Core.Documents;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Implementation.Services.QuestionnairePostProcessors;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer
{
    [TestOf(typeof(ListViewPostProcessor))]
    [TestFixture]
    public class QuestionnaireListViewPostProcessorTests
    {
        [Test]
        public void When_CloneQuestionnaire_command_Then_()
        {
            // arrange
            AssemblyContext.SetupServiceLocator();
            var listViewItemsStorage = new TestPlainStorage<QuestionnaireListViewItem>();
            Setup.InstanceToMockedServiceLocator<IPlainStorageAccessor<QuestionnaireListViewItem>>(listViewItemsStorage);

            Guid questionnaireId = Guid.NewGuid();
            var command = new CloneQuestionnaire(questionnaireId, "title", Guid.NewGuid(), true,
                Create.QuestionnaireDocumentWithOneChapter(questionnaireId));
            
            var listViewPostProcessor = CreateListViewPostProcessor();
            // act
            listViewPostProcessor.Process(null, command);

            // assert
            var questionnaireListViewItem = listViewItemsStorage.GetById(questionnaireId.FormatGuid());
            Assert.That(questionnaireListViewItem, Is.Not.Null);
            Assert.That(questionnaireListViewItem.Title, Is.EqualTo(command.Title));
            Assert.That(questionnaireListViewItem.IsPublic, Is.EqualTo(command.IsPublic));
            Assert.That(questionnaireListViewItem.CreatedBy, Is.EqualTo(command.ResponsibleId));
        }

        private static ListViewPostProcessor CreateListViewPostProcessor() => new ListViewPostProcessor();
    }
}