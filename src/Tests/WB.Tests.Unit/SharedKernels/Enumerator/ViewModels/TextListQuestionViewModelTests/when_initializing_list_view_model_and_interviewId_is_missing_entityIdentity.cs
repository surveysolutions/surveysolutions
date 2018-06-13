using System;
using Moq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;


namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.TextListQuestionViewModelTests
{
    internal class when_initializing_list_view_model_and_interviewId_is_missing_entityIdentity : TextListQuestionViewModelTestContext
    {
        [NUnit.Framework.Test] public void should_throw_exception() {
            var questionStateMock = new Mock<QuestionStateViewModel<TextListQuestionAnswered>>();
            var answeringViewModelMock = new Mock<AnsweringViewModel>();
            listModel = CreateTextListQuestionViewModel(questionStateMock.Object, answeringViewModelMock.Object);
            Assert.Throws<ArgumentNullException>(() => listModel.Init("Some interviewId", null, null));
        }

        static TextListQuestionViewModel listModel;
    }
}
