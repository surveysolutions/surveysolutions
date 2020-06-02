using System;
using System.Collections.Generic;
using System.Text;
using AutoFixture;
using Moq;
using NUnit.Framework;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Services;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Native.WebInterview
{
    class InterviewLifecycleEventHandlerTests
    {
        [Test]
        public void when_handling_prototype_of_TranslationSwitched_event_should_not_notify_connected_clients()
        {
            var fixture = Create.Other.AutoFixture();
            fixture.FreezeMock<IWebInterviewNotificationService>();

            var sut = fixture.Create<InterviewLifecycleEventHandler>();

            var @event = Create.PublishedEvent.TranslationSwitched(origin: "prototype");
            
            sut.Handle(@event);
            
            fixture.GetMock<IWebInterviewNotificationService>()
                .Verify(m => m.ReloadInterview(It.IsAny<Guid>()), Times.Never);
        }
        
        [Test]
        public void when_handling_TranslationSwitched_event_should_notify_connected_clients()
        {
            var fixture = Create.Other.AutoFixture();
            fixture.FreezeMock<IWebInterviewNotificationService>();

            var sut = fixture.Create<InterviewLifecycleEventHandler>();

            var @event = Create.PublishedEvent.TranslationSwitched();
            
            sut.Handle(@event);
            
            fixture.GetMock<IWebInterviewNotificationService>()
                .Verify(m => m.ReloadInterview(It.IsAny<Guid>()), Times.Once);
        }
    }
}
