using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.CalendarEvents;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Controllers;
using WB.UI.Headquarters.Controllers.Services;
using WB.UI.Headquarters.Controllers.Api.WebInterview;
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

        [TestCase(false)]
        [TestCase(true)]
        public void InterviewCommandsController_should_return_handled_expired_error_when_interview_is_missing(bool isReviewMode)
        {
            var commandService = new Mock<ICommandService>();
            var repository = new Mock<IStatefulInterviewRepository>();
            repository.Setup(x => x.Get(Id.g1.FormatGuid())).Returns((IStatefulInterview)null);
            var authorizedUser = Mock.Of<IAuthorizedUser>(x =>
                x.Id == Id.g3
                && x.IsAuthenticated == isReviewMode
                && x.IsAdministrator == isReviewMode);
            var controller = CreateInterviewCommandsController(commandService.Object, repository.Object, authorizedUser, isReviewMode);

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

        private static InterviewCommandsController CreateInterviewCommandsController(ICommandService commandService,
            IStatefulInterviewRepository repository, IAuthorizedUser authorizedUser, bool isReviewMode)
        {
            var controller = new InterviewCommandsController(commandService,
                Mock.Of<IImageFileStorage>(),
                Mock.Of<IAudioFileStorage>(),
                Mock.Of<IQuestionnaireStorage>(),
                repository,
                Mock.Of<IWebInterviewNotificationService>(),
                authorizedUser,
                Mock.Of<IInterviewFactory>(),
                Mock.Of<IUserViewFactory>(),
                Mock.Of<ICalendarEventService>(),
                Mock.Of<IAssignmentsService>(),
                Mock.Of<IPlainStorageAccessor<QuestionnaireBrowseItem>>(),
                Mock.Of<IHttpContextAccessor>());

            var httpContext = new DefaultHttpContext();
            if (isReviewMode)
                httpContext.Request.Headers["review"] = "true";

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            return controller;
        }
    }
}
