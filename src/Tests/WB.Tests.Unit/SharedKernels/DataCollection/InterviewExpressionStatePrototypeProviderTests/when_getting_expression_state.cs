using System;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewExpressionStatePrototypeProviderTests
{
    internal class when_getting_expression_state : InterviewExpressionStatePrototypeProviderTestContext
    {
        [Test]
        public void should_provide_not_null_value()
        {
            Guid id = Guid.Parse("33332222111100000000111122223333");
            long version = 3;

            var path = typeof(IInterviewExpressionState).Assembly.Location;

            AssemblyContext.SetupServiceLocator();

            var questionnareAssemblyFileAccessorMock = CreateIQuestionnareAssemblyFileAccessorMock(path);

            var interviewExpressionStatePrototype =
                CreateInterviewExpressionStatePrototype(questionnareAssemblyFileAccessorMock.Object);

            var isResultNotNull = interviewExpressionStatePrototype.GetExpressionState(id, version) != null;
            isResultNotNull.Should().BeTrue();
        }

    }
}
