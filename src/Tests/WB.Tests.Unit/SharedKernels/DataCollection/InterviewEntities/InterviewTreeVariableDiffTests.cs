using System;
using Moq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewEntities
{
    [TestFixture]
    public class InterviewTreeVariableDiffTests
    {
        [TestCase(null, null, false)]

        [TestCase(1, null, true)]
        [TestCase(null, 1, true)]
        [TestCase(1, 1, false)]
        [TestCase(1, 2, true)]

        [TestCase(1.1, null, true)]
        [TestCase(null, 1.1, true)]
        [TestCase(1.1, 1.1, false)]
        [TestCase(1.1, 1.12, true)]

        [TestCase(true, null, true)]
        [TestCase(null, true, true)]
        [TestCase(true, true, false)]
        [TestCase(true, false, true)]

        [TestCase("string", null, true)]
        [TestCase(null, "string", true)]
        [TestCase("string", "string", false)]
        [TestCase("string", "new string", true)]
        public void IsValueChanged_when_compare_variables_values(object sourceValue, object targetValue, bool isChangedResult)
        {
            var sourceVariable = Create.Entity.InterviewTreeVariable(Create.Entity.Identity(), value: sourceValue);
            var targetVariable = Create.Entity.InterviewTreeVariable(Create.Entity.Identity(), value: targetValue);

            var diff = Create.Entity.InterviewTreeVariableDiff(sourceVariable, targetVariable);

            Assert.AreEqual(diff.IsValueChanged, isChangedResult);
        }

        [Test]
        public void IsValueChanged_when_compare_datetime_variables_values()
        {
            Assert.AreEqual(IsValueChanged(DateTime.Now, null), true);
            Assert.AreEqual(IsValueChanged(null, DateTime.Now), true);
            Assert.AreEqual(IsValueChanged(DateTime.MinValue, DateTime.MinValue), false);
            Assert.AreEqual(IsValueChanged(DateTime.MinValue, DateTime.Now), true);
        }

        private bool IsValueChanged(object sourceValue, object targetValue)
        {
            var sourceVariable = Create.Entity.InterviewTreeVariable(Create.Entity.Identity(), value: sourceValue);
            var targetVariable = Create.Entity.InterviewTreeVariable(Create.Entity.Identity(), value: targetValue);

            var diff = Create.Entity.InterviewTreeVariableDiff(sourceVariable, targetVariable);
            return diff.IsValueChanged;
        }
    }
}