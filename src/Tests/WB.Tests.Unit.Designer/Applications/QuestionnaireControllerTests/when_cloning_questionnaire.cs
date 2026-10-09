using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.Scenarios;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.Infrastructure.CommandBus;
using WB.UI.Designer.Controllers;

namespace WB.Tests.Unit.Designer.Applications.QuestionnaireControllerTests
{
    [TestFixture]
    [TestOf(typeof(QuestionnaireController))]
    internal class when_cloning_questionnaire : QuestionnaireControllerTestContext
    {
        [Test]
        public async Task and_include_scenarios_is_true_should_copy_scenarios_to_new_questionnaire()
        {
            var sourceQuestionnaireId = Guid.NewGuid();
            var sourceDocument = Create.QuestionnaireDocument(id: sourceQuestionnaireId);

            var dbContext = Create.InMemoryDbContext();
            dbContext.Scenarios.AddRange(
                new StoredScenario { QuestionnaireId = sourceQuestionnaireId, Title = "Scenario 1", Steps = "[{\"step\":1}]" },
                new StoredScenario { QuestionnaireId = sourceQuestionnaireId, Title = "Scenario 2", Steps = "[{\"step\":2}]" }
            );
            await dbContext.SaveChangesAsync();

            var questionnaireView = Create.QuestionnaireView(sourceDocument);
            var viewFactoryMock = new Mock<IQuestionnaireViewFactory>();
            viewFactoryMock.Setup(f => f.Load(It.IsAny<QuestionnaireRevision>())).Returns(questionnaireView);

            var controller = CreateQuestionnaireController(
                commandService: Mock.Of<ICommandService>(),
                questionnaireViewFactory: viewFactoryMock.Object,
                dbContext: dbContext);
            controller.SetupLoggedInUser(Guid.NewGuid());

            var model = new QuestionnaireController.QuestionnaireCloneModel
            {
                QuestionnaireId = sourceQuestionnaireId,
                Title = "Copy of questionnaire",
                IncludeScenarios = true
            };

            await controller.Clone(model);

            var allScenarios = dbContext.Scenarios.ToList();
            allScenarios.Should().HaveCount(4); // 2 original + 2 copied
            var copiedScenarios = allScenarios.Where(s => s.QuestionnaireId != sourceQuestionnaireId).ToList();
            copiedScenarios.Should().HaveCount(2);
            copiedScenarios.Should().ContainSingle(s => s.Title == "Scenario 1" && s.Steps == "[{\"step\":1}]");
            copiedScenarios.Should().ContainSingle(s => s.Title == "Scenario 2" && s.Steps == "[{\"step\":2}]");
        }

        [Test]
        public async Task and_include_scenarios_is_false_should_not_copy_scenarios()
        {
            var sourceQuestionnaireId = Guid.NewGuid();
            var sourceDocument = Create.QuestionnaireDocument(id: sourceQuestionnaireId);

            var dbContext = Create.InMemoryDbContext();
            dbContext.Scenarios.AddRange(
                new StoredScenario { QuestionnaireId = sourceQuestionnaireId, Title = "Scenario 1", Steps = "[{\"step\":1}]" }
            );
            await dbContext.SaveChangesAsync();

            var questionnaireView = Create.QuestionnaireView(sourceDocument);
            var viewFactoryMock = new Mock<IQuestionnaireViewFactory>();
            viewFactoryMock.Setup(f => f.Load(It.IsAny<QuestionnaireRevision>())).Returns(questionnaireView);

            var controller = CreateQuestionnaireController(
                commandService: Mock.Of<ICommandService>(),
                questionnaireViewFactory: viewFactoryMock.Object,
                dbContext: dbContext);
            controller.SetupLoggedInUser(Guid.NewGuid());

            var model = new QuestionnaireController.QuestionnaireCloneModel
            {
                QuestionnaireId = sourceQuestionnaireId,
                Title = "Copy of questionnaire",
                IncludeScenarios = false
            };

            await controller.Clone(model);

            var allScenarios = dbContext.Scenarios.ToList();
            allScenarios.Should().HaveCount(1); // only the original
            allScenarios.Single().QuestionnaireId.Should().Be(sourceQuestionnaireId);
        }

        [Test]
        public async Task and_include_scenarios_is_true_cloned_scenarios_should_have_new_questionnaire_id()
        {
            var sourceQuestionnaireId = Guid.NewGuid();
            var sourceDocument = Create.QuestionnaireDocument(id: sourceQuestionnaireId);

            var dbContext = Create.InMemoryDbContext();
            dbContext.Scenarios.Add(
                new StoredScenario { QuestionnaireId = sourceQuestionnaireId, Title = "My Scenario", Steps = "[{}]" }
            );
            await dbContext.SaveChangesAsync();

            var questionnaireView = Create.QuestionnaireView(sourceDocument);
            var viewFactoryMock = new Mock<IQuestionnaireViewFactory>();
            viewFactoryMock.Setup(f => f.Load(It.IsAny<QuestionnaireRevision>())).Returns(questionnaireView);

            var controller = CreateQuestionnaireController(
                commandService: Mock.Of<ICommandService>(),
                questionnaireViewFactory: viewFactoryMock.Object,
                dbContext: dbContext);
            controller.SetupLoggedInUser(Guid.NewGuid());

            var model = new QuestionnaireController.QuestionnaireCloneModel
            {
                QuestionnaireId = sourceQuestionnaireId,
                Title = "Copy of questionnaire",
                IncludeScenarios = true
            };

            await controller.Clone(model);

            var copiedScenario = dbContext.Scenarios.Single(s => s.QuestionnaireId != sourceQuestionnaireId);
            copiedScenario.QuestionnaireId.Should().NotBe(sourceQuestionnaireId);
            copiedScenario.Title.Should().Be("My Scenario");
            copiedScenario.Steps.Should().Be("[{}]");
        }

        [Test]
        public async Task and_command_is_rejected_should_mark_request_rollback_only()
        {
            var sourceQuestionnaireId = Guid.NewGuid();
            var questionnaireView = Create.QuestionnaireView(Create.QuestionnaireDocument(id: sourceQuestionnaireId));
            var viewFactoryMock = new Mock<IQuestionnaireViewFactory>();
            viewFactoryMock.Setup(f => f.Load(It.IsAny<QuestionnaireRevision>())).Returns(questionnaireView);

            var commandService = new Mock<ICommandService>();
            commandService
                .Setup(s => s.Execute(It.IsAny<ICommand>(), It.IsAny<string>()))
                .Throws(new QuestionnaireException(DomainExceptionType.DoesNotHavePermissionsForEdit, "Forbidden"));

            var rollbackState = new TransactionRollbackState();
            var controller = CreateQuestionnaireController(
                commandService: commandService.Object,
                questionnaireViewFactory: viewFactoryMock.Object,
                transactionRollbackState: rollbackState);
            controller.SetupLoggedInUser(Guid.NewGuid());

            var model = new QuestionnaireController.QuestionnaireCloneModel
            {
                QuestionnaireId = sourceQuestionnaireId,
                Title = "Copy of questionnaire"
            };

            await controller.Clone(model);

            rollbackState.IsRollbackOnly.Should().BeTrue();
        }

        [Test]
        public void and_creation_command_is_rejected_should_mark_request_rollback_only()
        {
            var commandService = new Mock<ICommandService>();
            commandService
                .Setup(s => s.Execute(It.IsAny<ICommand>(), It.IsAny<string>()))
                .Throws(new QuestionnaireException(DomainExceptionType.DoesNotHavePermissionsForEdit, "Forbidden"));

            var rollbackState = new TransactionRollbackState();
            var controller = CreateQuestionnaireController(
                commandService: commandService.Object,
                transactionRollbackState: rollbackState);
            controller.SetupLoggedInUser(Guid.NewGuid());

            controller.Create(new QuestionnaireController.QuestionnaireViewModel { Title = "New questionnaire", Variable = "q1" });

            rollbackState.IsRollbackOnly.Should().BeTrue();
        }
    }
}
