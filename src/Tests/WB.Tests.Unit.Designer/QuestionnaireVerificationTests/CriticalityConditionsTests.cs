using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Tests.Abc;
using WB.Tests.Unit.Designer;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    [TestFixture]
    public class CriticalityConditionsTests
    {
        [Test]
        public void criticality_conditions_has_empty_message()
            => Create.QuestionnaireDocumentWithOneChapter()
                .AddCriticalityCondition()
                .ExpectError("WB0320");

        [Test]
        public void criticality_conditions_has_empty_expression()
            => Create.QuestionnaireDocumentWithOneChapter()
                .AddCriticalityCondition()
                .ExpectError("WB0319");

        [Test]
        public void criticality_conditions_has_DateTimeNoe_in_expression()
            => Create.QuestionnaireDocumentWithOneChapter()
                .AddCriticalityCondition("DateTime.Now > 10")
                .ExpectError("WB0118");

        [Test]
        public void criticality_conditions_has_forbidden_class_in_expression()
            => Create.QuestionnaireDocumentWithOneChapter()
                .AddCriticalityCondition("i < 10; System.GC.Collect(); System.Activator(\"class\");")
                .ExpectError("WB0321");

        [Test]
        public void criticality_conditions_has_BitwiseAnd_in_expression()
            => Create.QuestionnaireDocumentWithOneChapter()
                .AddCriticalityCondition("i < 10; i & j")
                .ExpectWarning("WB0237");

        [Test]
        public void criticality_conditions_has_BitwiseOr_in_expression()
            => Create.QuestionnaireDocumentWithOneChapter()
                .AddCriticalityCondition("i < 10; i | j")
                .ExpectWarning("WB0238");
    }
}

public static class CriticalityConditionsTestsExtensions
{
    public static QuestionnaireDocument AddCriticalityCondition(
        this QuestionnaireDocument questionnaire, string expression = null, string message = null)
    {
        questionnaire.CriticalRules.Add(new CriticalRule()
        {
            Id = Guid.NewGuid(),
            Message = message,
            Expression = expression,
        });
        return questionnaire;
    }
}
