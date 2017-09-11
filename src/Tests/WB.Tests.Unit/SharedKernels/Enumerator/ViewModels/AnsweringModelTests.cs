using Moq;
using NUnit.Framework;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels
{
    [TestOf(typeof(AnsweringViewModel))]
    public class AnsweringModelTests
    {
        [Test]
        public void should_not_shallow_exception_InterviewException()
        {
            var commandServiceMock = new Mock<ICommandService>();
            commandServiceMock
                .Setup(cs => cs.Execute(It.IsAny<ICommand>(), It.IsAny<string>()))
                .Throws(new InterviewException("Some message that should be passed up"));

            var answering = new AnsweringViewModel(
                commandServiceMock.Object, 
                Mock.Of<IUserInterfaceStateService>());

            Assert.Throws<InterviewException>(async () => 
                await answering.SendAnswerQuestionCommandAsync(
                    Create.Command.AnswerNumericRealQuestionCommand(Id.g1, Id.g2, 1)));
        }
    }
}