using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Controllers;
using WB.Tests.Abc;

namespace WB.Tests.Web.Headquarters.Controllers.WebInterview
{
    public class CommandsControllerTests
    {
        [Test]
        public void AnswerSingleOptionQuestion_returns_handled_expired_error_when_interview_is_missing()
        {
            var commandService = new Mock<ICommandService>();
            var repository = new Mock<IStatefulInterviewRepository>();
            repository.Setup(x => x.Get(Id.g1.FormatGuid())).Returns((IStatefulInterview)null);
            var controller = CreateController(commandService.Object, repository.Object);

            var exception = Assert.Throws<InterviewException>(() => controller.AnswerSingleOptionQuestion(Id.g1,
                new CommandsController.AnswerRequest<int?>
                {
                    Identity = new Identity(Id.g2, RosterVector.Empty).ToString(),
                    Answer = 1
                }));

            Assert.That(exception?.ExceptionType, Is.EqualTo(InterviewDomainExceptionType.InterviewHardDeleted));
            commandService.Verify(x => x.Execute(It.IsAny<ICommand>(), It.IsAny<string>()), Times.Never);

            var context = new ExceptionContext(
                new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()),
                new List<IFilterMetadata>()) { Exception = exception };

            new HandleCommandErrorAttribute().OnException(context);

            Assert.That(context.ExceptionHandled, Is.True);
            Assert.That(context.Result, Is.TypeOf<JsonResult>());
            var result = (JsonResult)context.Result;
            Assert.That(result?.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
            Assert.That(result?.Value?.GetType().GetProperty("ErrorMessage")?.GetValue(result.Value),
                Is.EqualTo(Enumerator.Native.Resources.WebInterview.Error_InterviewExpired));
        }

        [Test]
        public void AnswerSingleOptionQuestion_executes_command_with_current_responsible_when_interview_exists()
        {
            var commandService = new Mock<ICommandService>();
            var interview = Mock.Of<IStatefulInterview>(x => x.CurrentResponsibleId == Id.g3);
            var repository = new Mock<IStatefulInterviewRepository>();
            repository.Setup(x => x.Get(Id.g1.FormatGuid())).Returns(interview);
            var controller = CreateController(commandService.Object, repository.Object);

            var result = controller.AnswerSingleOptionQuestion(Id.g1, new CommandsController.AnswerRequest<int?>
            {
                Identity = new Identity(Id.g2, RosterVector.Empty).ToString(),
                Answer = 1
            });

            Assert.That(result, Is.TypeOf<OkResult>());
            commandService.Verify(x => x.Execute(It.Is<AnswerSingleOptionQuestionCommand>(
                command => command.InterviewId == Id.g1 && command.UserId == Id.g3), null), Times.Once);
        }

        private static CommandsController CreateController(ICommandService commandService,
            IStatefulInterviewRepository repository)
        {
            return new Mock<CommandsController>(commandService,
                Mock.Of<IImageFileStorage>(), Mock.Of<IAudioFileStorage>(),
                Mock.Of<IQuestionnaireStorage>(), repository, Mock.Of<IWebInterviewNotificationService>())
            {
                CallBase = true
            }.Object;
        }
    }
}

