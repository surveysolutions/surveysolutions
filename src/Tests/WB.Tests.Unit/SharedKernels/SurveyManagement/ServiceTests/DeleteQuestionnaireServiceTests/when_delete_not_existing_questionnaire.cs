using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DeleteQuestionnaireTemplate;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DeleteQuestionnaireServiceTests
{
    internal class when_delete_not_existing_questionnaire : DeleteQuestionnaireServiceTestContext
    {
        Establish context = () =>
        {
            commandServiceMock=new Mock<ICommandService>();
            deleteQuestionnaireService = CreateDeleteQuestionnaireService(commandService: commandServiceMock.Object);
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                deleteQuestionnaireService.DeleteQuestionnaire(questionnaireId, questionnaireVersion, userId));

        It should_throw_ArgumentException = () =>
            exception.ShouldBeOfExactType<ArgumentException>();

        private static DeleteQuestionnaireService deleteQuestionnaireService;
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static long questionnaireVersion = 5;
        private static Guid userId = Guid.Parse("22222222222222222222222222222222");
        private static Mock<ICommandService> commandServiceMock;
        private static Exception exception;
    }
}
