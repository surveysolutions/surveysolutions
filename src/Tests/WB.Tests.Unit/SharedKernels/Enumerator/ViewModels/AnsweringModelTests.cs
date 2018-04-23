using System.Threading;
using System.Threading.Tasks;
using Moq;
using MvvmCross.Plugin.Messenger;
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
        public async Task should_not_shallow_exception_InterviewException()
        {
            var commandServiceMock = new Mock<ICommandService>();
            commandServiceMock
                .Setup(cs => cs.ExecuteAsync(It.IsAny<ICommand>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws(new InterviewException("Some message that should be passed up"));

            var answering = new AnsweringViewModel(
                commandServiceMock.Object, 
                Mock.Of<IUserInterfaceStateService>(),
                Mock.Of<IMvxMessenger>());

            try
            {
                await answering.SendAnswerQuestionCommandAsync(
                    Create.Command.AnswerNumericRealQuestionCommand(Id.g1, Id.g2, 1));
            }
            catch (InterviewException)
            {
                Assert.Pass();
            }

            Assert.Fail("Should throw InterviewException");
        }
    }
}
