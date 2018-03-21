using System;
using FluentAssertions;
using Machine.Specifications;
using NUnit.Framework;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;


namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.CascadingSingleOptionQuestionViewModelTests
{
    internal class when_initializing_cascading_view_model_and_interviewId_is_missing_entityIdentity : CascadingSingleOptionQuestionViewModelTestContext
    {
        [NUnit.Framework.Test] public void should_throw_exception_with_ParamName_equals_interviewId () {
            SetUp();
            cascadingModel = CreateCascadingSingleOptionQuestionViewModel();

            var exception = Assert.Throws<ArgumentNullException>(() => cascadingModel.Init(null, null, null));
            exception.ParamName.Should().Be("interviewId");

        }

        static CascadingSingleOptionQuestionViewModel cascadingModel;
    }
}
