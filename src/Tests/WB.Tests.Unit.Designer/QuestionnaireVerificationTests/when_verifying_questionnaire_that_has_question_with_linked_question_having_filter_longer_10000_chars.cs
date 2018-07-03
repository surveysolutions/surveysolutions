using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;


namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_that_has_question_with_linked_question_having_filter_longer_10000_chars : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaireDocument(new IComposite[]
            {
                Create.FixedRoster(rosterId: rosterId, fixedTitles: new[] { "1", "2", "3" },children: new[]
                {
                    Create.NumericIntegerQuestion(variable: "enumeration_district"),
                }),

                Create.Group(groupId: groupId, children: new[]
                {
                    Create.SingleOptionQuestion(questionId: questionId, 
                    variable: "s546i",
                    linkedToRosterId: rosterId,
                    linkedFilterExpression: "(enumeration_district == 100) || (enumeration_district == 200) || (enumeration_district == 300) || (enumeration_district == 400) || (enumeration_district == 501) || (enumeration_district == 502) || (enumeration_district == 600) || (enumeration_district == 701) || (enumeration_district == 702) || (enumeration_district == 801) || (enumeration_district == 802) || (enumeration_district == 901) || (enumeration_district == 902) || (enumeration_district == 903) || (enumeration_district == 1001) || (enumeration_district == 1002) || (enumeration_district == 1101) || (enumeration_district == 1102) || (enumeration_district == 1201) || (enumeration_district == 1202) || (enumeration_district == 1301) || (enumeration_district == 1302) || (enumeration_district == 1401) || (enumeration_district == 1402) || (enumeration_district == 1403) || (enumeration_district == 1404) || (enumeration_district == 1405) || (enumeration_district == 1406) || (enumeration_district == 1407) || (enumeration_district == 1408) || (enumeration_district == 1409) || (enumeration_district == 1410) || (enumeration_district == 1411) || (enumeration_district == 1412) || (enumeration_district == 1413) || (enumeration_district == 1500) || (enumeration_district == 1601) || (enumeration_district == 1602) || (enumeration_district == 1701) || (enumeration_district == 1702) || (enumeration_district == 1703) || (enumeration_district == 1800) || (enumeration_district == 1901) || (enumeration_district == 1902) || (enumeration_district == 1903) || (enumeration_district == 1904) || (enumeration_district == 2000) || (enumeration_district == 2101) || (enumeration_district == 2102) || (enumeration_district == 2103) || (enumeration_district == 2104) || (enumeration_district == 2105) || (enumeration_district == 2106) || (enumeration_district == 2107) || (enumeration_district == 2201) || (enumeration_district == 2202) || (enumeration_district == 2203) || (enumeration_district == 2301) || (enumeration_district == 2302) || (enumeration_district == 2303) || (enumeration_district == 2304) || (enumeration_district == 2305) || (enumeration_district == 2401) || (enumeration_district == 2402) || (enumeration_district == 2403) || (enumeration_district == 2404) || (enumeration_district == 2501) || (enumeration_district == 2502) || (enumeration_district == 2503) || (enumeration_district == 2504) || (enumeration_district == 2505) || (enumeration_district == 2601) || (enumeration_district == 2602) || (enumeration_district == 2603) || (enumeration_district == 2604) || (enumeration_district == 2605) || (enumeration_district == 2606) || (enumeration_district == 2607) || (enumeration_district == 2608) || (enumeration_district == 2701) || (enumeration_district == 2702) || (enumeration_district == 2703) || (enumeration_district == 2704) || (enumeration_district == 2705) || (enumeration_district == 2706) || (enumeration_district == 2801) || (enumeration_district == 2802) || (enumeration_district == 2803) || (enumeration_district == 2804) || (enumeration_district == 2805) || (enumeration_district == 2806) || (enumeration_district == 2807) || (enumeration_district == 2808) || (enumeration_district == 2809) || (enumeration_district == 2810) || (enumeration_district == 2901) || (enumeration_district == 2902) || (enumeration_district == 3001) || (enumeration_district == 3002) || (enumeration_district == 3101) || (enumeration_district == 3102) || (enumeration_district == 3103) || (enumeration_district == 3104) || (enumeration_district == 3105) || (enumeration_district == 3201) || (enumeration_district == 3202) || (enumeration_district == 3203) || (enumeration_district == 3301) || (enumeration_district == 3302) || (enumeration_district == 3401) || (enumeration_district == 3402) || (enumeration_district == 3403) || (enumeration_district == 3404) || (enumeration_district == 3405) || (enumeration_district == 3406) || (enumeration_district == 3407) || (enumeration_district == 3408) || (enumeration_district == 3409) || (enumeration_district == 3410) || (enumeration_district == 3501) || (enumeration_district == 3502) || (enumeration_district == 3503) || (enumeration_district == 3504) || (enumeration_district == 3505) || (enumeration_district == 3506) || (enumeration_district == 3507) || (enumeration_district == 3508) || (enumeration_district == 3509) || (enumeration_district == 3601) || (enumeration_district == 3602) || (enumeration_district == 3701) || (enumeration_district == 3702) || (enumeration_district == 3703) || (enumeration_district == 3704) || (enumeration_district == 3705) || (enumeration_district == 3706) || (enumeration_district == 3801) || (enumeration_district == 3802) || (enumeration_district == 3803) || (enumeration_district == 3804) || (enumeration_district == 3805) || (enumeration_district == 3901) || (enumeration_district == 3902) || (enumeration_district == 3903) || (enumeration_district == 3904) || (enumeration_district == 3905) || (enumeration_district == 4001) || (enumeration_district == 4002) || (enumeration_district == 4003) || (enumeration_district == 4004) || (enumeration_district == 4005) || (enumeration_district == 4006) || (enumeration_district == 4007) || (enumeration_district == 4100) || (enumeration_district == 4201) || (enumeration_district == 4202) || (enumeration_district == 4203) || (enumeration_district == 4204) || (enumeration_district == 4301) || (enumeration_district == 4302) || (enumeration_district == 4304) || (enumeration_district == 4401) || (enumeration_district == 4402) || (enumeration_district == 4403) || (enumeration_district == 4404) || (enumeration_district == 4501) || (enumeration_district == 4502) || (enumeration_district == 4503) || (enumeration_district == 4504) || (enumeration_district == 4600) || (enumeration_district == 4701) || (enumeration_district == 4702) || (enumeration_district == 4801) || (enumeration_district == 4802) || (enumeration_district == 4803) || (enumeration_district == 4804) || (enumeration_district == 4805) || (enumeration_district == 4806) || (enumeration_district == 4807) || (enumeration_district == 4808) || (enumeration_district == 4809) || (enumeration_district == 4811) || (enumeration_district == 4901) || (enumeration_district == 4902) || (enumeration_district == 4903) || (enumeration_district == 4904) || (enumeration_district == 4905) || (enumeration_district == 4906) || (enumeration_district == 4907) || (enumeration_district == 4908) || (enumeration_district == 4909) || (enumeration_district == 4910) || (enumeration_district == 4911) || (enumeration_district == 4912) || (enumeration_district == 4913) || (enumeration_district == 4914) || (enumeration_district == 4915) || (enumeration_district == 4916) || (enumeration_district == 4917) || (enumeration_district == 4918) || (enumeration_district == 4919) || (enumeration_district == 4920) || (enumeration_district == 4921) || (enumeration_district == 4922) || (enumeration_district == 4923) || (enumeration_district == 5001) || (enumeration_district == 5002) || (enumeration_district == 5003) || (enumeration_district == 5004) || (enumeration_district == 5005) || (enumeration_district == 5006) || (enumeration_district == 5100) || (enumeration_district == 5200) || (enumeration_district == 5301) || (enumeration_district == 5302) || (enumeration_district == 5400) || (enumeration_district == 5500) || (enumeration_district == 5600) || (enumeration_district == 5700) || (enumeration_district == 5801) || (enumeration_district == 5802) || (enumeration_district == 5901) || (enumeration_district == 5902) || (enumeration_district == 6001) || (enumeration_district == 6002) || (enumeration_district == 6101) || (enumeration_district == 6102) || (enumeration_district == 6201) || (enumeration_district == 6202) || (enumeration_district == 6203) || (enumeration_district == 6204) || (enumeration_district == 6205) || (enumeration_district == 6301) || (enumeration_district == 6302) || (enumeration_district == 6401) || (enumeration_district == 6402) || (enumeration_district == 6501) || (enumeration_district == 6502) || (enumeration_district == 6503) || (enumeration_district == 6504) || (enumeration_district == 6601) || (enumeration_district == 6602) || (enumeration_district == 6701) || (enumeration_district == 6702) || (enumeration_district == 6703) || (enumeration_district == 6704) || (enumeration_district == 6801) || (enumeration_district == 6802) || (enumeration_district == 6901) || (enumeration_district == 6902) || (enumeration_district == 6903) || (enumeration_district == 6904) || (enumeration_district == 7001) || (enumeration_district == 7002) || (enumeration_district == 7101) || (enumeration_district == 7102) || (enumeration_district == 7103) || (enumeration_district == 7200) || (enumeration_district == 7301) || (enumeration_district == 7302) || (enumeration_district == 7400) || (enumeration_district == 7501) || (enumeration_district == 7502) || (enumeration_district == 7503) || (enumeration_district == 7600) || (enumeration_district == 7700) || (enumeration_district == 7800) || (enumeration_district == 7900) || (enumeration_district == 8001) || (enumeration_district == 8002) || (enumeration_district == 8101) || (enumeration_district == 8102) || (enumeration_district == 8103) || (enumeration_district == 8200) || (enumeration_district == 8300) || (enumeration_district == 8400) || (enumeration_district == 8501) || (enumeration_district == 8502) || (enumeration_district == 8601) || (enumeration_district == 8602) || (enumeration_district == 8700) || (enumeration_district == 8801) || (enumeration_district == 8802) || (enumeration_district == 8901) || (enumeration_district == 8902) || (enumeration_district == 8903) || (enumeration_district == 9001) || (enumeration_district == 9002) || (enumeration_district == 9003) || (enumeration_district == 9004) || (enumeration_district == 9005) || (enumeration_district == 9101) || (enumeration_district == 9102) || (enumeration_district == 9200) || (enumeration_district == 9300) || (enumeration_district == 9401) || (enumeration_district == 9402) || (enumeration_district == 9403) || (enumeration_district == 9500) || (enumeration_district == 9601) || (enumeration_district == 9602) || (enumeration_district == 9701) || (enumeration_district == 9702) || (enumeration_district == 9801) || (enumeration_district == 9802) || (enumeration_district == 9900)")
                })
            });

            questionnireExpressionProcessorGeneratorMock = new Mock<IExpressionProcessorGenerator>();
            string generationResult;
            questionnireExpressionProcessorGeneratorMock.Setup(
                _ => _.GenerateProcessorStateAssembly(Moq.It.IsAny<QuestionnaireDocument>(), Moq.It.IsAny<int>(), out generationResult))
                .Returns(new GenerationResult() { Success = true, Diagnostics = new List<GenerationDiagnostic>() });

            verifier = CreateQuestionnaireVerifier(expressionProcessorGenerator: questionnireExpressionProcessorGeneratorMock.Object);
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_1_message () =>
            verificationMessages.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_return_message_with_code__WB0108 () =>
            verificationMessages.First().Code.Should().Be("WB0108");

        [NUnit.Framework.Test] public void should_return_message_with_one_references () =>
            verificationMessages.First().References.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_return_message_with_one_references_with_question_type () =>
            verificationMessages.First().References.First().Type.Should().Be(QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_message_with_one_references_with_id_equals_questionId () =>
            verificationMessages.First().References.First().Id.Should().Be(questionId);

        [NUnit.Framework.Test] public void should_not_call_GenerateProcessorStateAssembly () =>
            questionnireExpressionProcessorGeneratorMock.Verify(x => x.GenerateProcessorStateAssembly(Moq.It.IsAny<QuestionnaireDocument>(), Moq.It.IsAny<int>(), out generationResult), Times.Never);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid rosterId = Guid.Parse("BAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");


        private static Mock<IExpressionProcessorGenerator> questionnireExpressionProcessorGeneratorMock;
        private static string generationResult;
    }
}