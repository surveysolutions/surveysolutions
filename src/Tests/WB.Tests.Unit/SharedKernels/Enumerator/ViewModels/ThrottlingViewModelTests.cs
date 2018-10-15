using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels
{
    [TestFixture]
    internal class ThrottlingViewModelTests
    {
        [Test]
        public async Task When_executing_throttled_action()
        {
            var userInterfaceMock = new Mock<IUserInterfaceStateService>();
            var model = Create.ViewModel.ThrottlingViewModel(userInterfaceStateService: userInterfaceMock.Object);
            model.ThrottlePeriod = 100;
            model.Init(async () => { await Task.CompletedTask; });

            // Act
            await model.ExecuteActionIfNeeded();

            userInterfaceMock.Verify(x => x.ThrottledActionStarted(), Times.Once);
        }

        [Test]
        public async Task When_cancelling_throttled_action()
        {
            var userInterfaceMock = new Mock<IUserInterfaceStateService>();
            var model = Create.ViewModel.ThrottlingViewModel(userInterfaceStateService: userInterfaceMock.Object);
            model.ThrottlePeriod = 100;
            model.Init(async () => { await Task.CompletedTask; });
            await model.ExecuteActionIfNeeded();

            // Act
            model.CancelPendingAction();

            userInterfaceMock.Verify(x => x.ThrottledActionFinished(), Times.Once);
        }

        [Test]
        public async Task When_action_is_finished()
        {
            var userInterfaceMock = new Mock<IUserInterfaceStateService>();
            var model = Create.ViewModel.ThrottlingViewModel(userInterfaceStateService: userInterfaceMock.Object);
            model.ThrottlePeriod = 100;
            model.Init(async () => { await Task.CompletedTask; });
            await model.ExecuteActionIfNeeded();

            // Act
            await Task.Delay(model.ThrottlePeriod);

            userInterfaceMock.Verify(x => x.ThrottledActionFinished(), Times.Once);
        }
    }
}
