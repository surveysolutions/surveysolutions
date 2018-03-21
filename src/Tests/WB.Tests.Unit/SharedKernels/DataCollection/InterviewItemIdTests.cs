using System;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Tests.Unit.SharedKernels.DataCollection
{
    internal class InterviewItemIdTestsContext
    {
        protected static InterviewItemId CreateInterviewItemId(Guid? id = null, decimal[] vector = null)
        {
            return new InterviewItemId(id ?? Guid.NewGuid(), vector ?? new decimal[0]);
        }

        protected static Guid InterviewItemIdInitialData
        {
            get { return Guid.Parse("00000000-0000-0000-0000-000000000000"); }
        }

        protected static Guid DifferentInterviewItemIdInitialData
        {
            get { return Guid.Parse("11111111-1111-1111-1111-111111111111"); }
        }
    }

    #region GetHashCode tests

    [NUnit.Framework.TestOf(typeof (InterviewItemId))]
    internal class GetHashCode_when_called_two_times_for_two_identical_InterviewItemIds_in_when_part : InterviewItemIdTestsContext
    {
        private Establish context = () =>
            {
                entity = CreateInterviewItemId(
                    InterviewItemIdInitialData);

                identicalInterviewItemId = CreateInterviewItemId(
                    InterviewItemIdInitialData);
            };

        Because of = () =>
            {
                firstResult = entity.GetHashCode();
                secondResult = identicalInterviewItemId.GetHashCode();
            };

        private It should_same_results_for_both_calls = () =>
                                                        firstResult.ShouldEqual(secondResult);

        private static int firstResult;
        private static int secondResult;
        private static InterviewItemId entity;
        private static InterviewItemId identicalInterviewItemId;
    }

    #endregion // GetHashCode tests

    #region Equals tests

    [NUnit.Framework.TestOf(typeof (InterviewItemId))]
    internal class Equals_when_called_with_null : InterviewItemIdTestsContext
    {
        private Establish context = () =>
            {
                entity = CreateInterviewItemId();
            };

        Because of = () =>
                             result = entity.Equals(null);

        private It should_return_false = () =>
                                         result.ShouldBeFalse();

        private static bool result;
        private static InterviewItemId entity;
    }

    [NUnit.Framework.TestOf(typeof (InterviewItemId))]
    internal class Equals_when_called_with_object_of_another_type : InterviewItemIdTestsContext
    {
        private Establish context = () =>
            {
                entity = CreateInterviewItemId();
            };

        Because of = () =>
                             result = entity.Equals(object_of_other_type_probably_string);

        private It should_return_false = () =>
                                         result.ShouldBeFalse();

        private const string object_of_other_type_probably_string = "hello world";
        private static bool result;
        private static InterviewItemId entity;
    }

    [NUnit.Framework.TestOf(typeof (InterviewItemId))]
    internal class Equals_when_called_with_identical_interviewItemId_in_when_part : InterviewItemIdTestsContext
    {
        private Establish context = () =>
            {
                entity = CreateInterviewItemId(
                    InterviewItemIdInitialData);

                identicalInterviewItemId = CreateInterviewItemId(
                    InterviewItemIdInitialData);
            };

        Because of = () =>
                             result = entity.Equals(identicalInterviewItemId);

        private It should_return_true = () =>
                                        result.ShouldBeTrue();

        private static bool result;
        private static InterviewItemId entity;
        private static InterviewItemId identicalInterviewItemId;
    }

    [NUnit.Framework.TestOf(typeof (InterviewItemId))]
    internal class Equals_when_called_with_different_interviewItemId_in_when_part : InterviewItemIdTestsContext
    {
        private Establish context = () =>
            {
                entity = CreateInterviewItemId(
                    InterviewItemIdInitialData);

                differentInterviewItemId = CreateInterviewItemId(
                    DifferentInterviewItemIdInitialData);
            };

        Because of = () =>
                             result = entity.Equals(differentInterviewItemId);

        private It should_return_false = () =>
                                         result.ShouldBeFalse();

        private static bool result;
        private static InterviewItemId entity;
        private static InterviewItemId differentInterviewItemId;
    }

    #endregion // Equals tests

    #region == operator tests

    [NUnit.Framework.TestOf(typeof (InterviewItemId))]
    internal class EqualityOperator_when_called_with_same_interviewItemId_in_when_part : InterviewItemIdTestsContext
    {
        private Establish context = () =>
            {
                entity = CreateInterviewItemId();
            };

#pragma warning disable 1718

        Because of = () =>
                             result = entity == entity;

#pragma warning restore 1718

        private It should_return_true = () =>
                                        result.ShouldBeTrue();

        private static bool result;
        private static InterviewItemId entity;
    }

    [NUnit.Framework.TestOf(typeof (InterviewItemId))]
    internal class EqualityOperator_when_called_with_two_identical_InterviewItemIds_in_when_part : InterviewItemIdTestsContext
    {
        private Establish context = () =>
            {
                entity = CreateInterviewItemId(
                    InterviewItemIdInitialData);

                identicalInterviewItemId = CreateInterviewItemId(
                    InterviewItemIdInitialData);
            };

        Because of = () =>
                             result = entity == identicalInterviewItemId;

        private It should_return_true = () =>
                                        result.ShouldBeTrue();

        private static bool result;
        private static InterviewItemId entity;
        private static InterviewItemId identicalInterviewItemId;
    }

    [NUnit.Framework.TestOf(typeof (InterviewItemId))]
    internal class EqualityOperator_when_called_with_two_different_InterviewItemIds_in_when_part : InterviewItemIdTestsContext
    {
        private Establish context = () =>
            {
                entity = CreateInterviewItemId(
                    InterviewItemIdInitialData);

                differentInterviewItemId = CreateInterviewItemId(
                    DifferentInterviewItemIdInitialData);
            };

        Because of = () =>
                             result = entity == differentInterviewItemId;

        private It should_return_false = () =>
                                         result.ShouldBeFalse();

        private static bool result;
        private static InterviewItemId entity;
        private static InterviewItemId differentInterviewItemId;
    }

    #endregion // == operator tests
}
