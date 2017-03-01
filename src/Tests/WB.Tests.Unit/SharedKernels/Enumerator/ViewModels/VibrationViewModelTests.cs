using System;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels
{
    [TestOf(typeof(VibrationViewModel))]
    public class VibrationViewModelTests
    {
        [Test]
        public void when_Initialize_twice_Then_event_registry_should_subscribe_view_model_once()
        {
            //arrange
            var interviewId = Guid.Parse("11111111111111111111111111111111").FormatGuid();
            var mockOfEventRegistry = new Mock<ILiteEventRegistry>();
            var vibrationViewModel = Create.ViewModel.VibrationViewModel(mockOfEventRegistry.Object);
            //act
            vibrationViewModel.Initialize(interviewId);
            //assert
            mockOfEventRegistry.Verify(x => x.Subscribe(vibrationViewModel, interviewId), Times.Once);
        }
    }
}