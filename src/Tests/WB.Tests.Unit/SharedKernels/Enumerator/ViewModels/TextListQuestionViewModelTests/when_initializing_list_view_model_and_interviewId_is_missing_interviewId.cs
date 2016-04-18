﻿using System;
using Machine.Specifications;
using Moq;
using Nito.AsyncEx;
using Nito.AsyncEx.Synchronous;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.TextListQuestionViewModelTests
{
    internal class when_initializing_list_view_model_and_interviewId_is_missing_interviewId : TextListQuestionViewModelTestContext
    {
        Establish context = () =>
        {
            var questionStateMock = new Mock<QuestionStateViewModel<TextListQuestionAnswered>>();
            var answeringViewModelMock = new Mock<AnsweringViewModel>();
            listModel = CreateTextListQuestionViewModel(questionStateMock.Object, answeringViewModelMock.Object);
        };

        Because of = () =>
            exception = Catch.Exception(() => listModel.Init(null, null, null));

        It should_throw_ArgumentNullException_exception = () =>
            exception.ShouldBeOfExactType<ArgumentNullException>();

        It should_throw_exception_with_ParamName_equals_interviewId = () =>
            (exception as ArgumentNullException).ParamName.ShouldEqual("interviewId");

        static TextListQuestionViewModel listModel;
        private static Exception exception;
    }
}