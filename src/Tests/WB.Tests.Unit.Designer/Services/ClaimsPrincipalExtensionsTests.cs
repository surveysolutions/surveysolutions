using System;
using System.Security.Claims;
using FluentAssertions;
using NUnit.Framework;
using WB.UI.Designer.Services;

namespace WB.Tests.Unit.Designer.Services
{
    [TestFixture]
    [TestOf(typeof(ClaimsPrincipalExtensions))]
    public class ClaimsPrincipalExtensionsTests
    {
        private static ClaimsPrincipal PrincipalWith(params Claim[] claims)
            => new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));

        [Test]
        public void HasMatchingQuestionnaireId_should_return_true_when_claim_matches()
        {
            var id = Guid.NewGuid();
            var user = PrincipalWith(new Claim(JwtTokenService.QuestionnaireIdClaimType, id.ToString()));

            user.HasMatchingQuestionnaireId(id).Should().BeTrue();
        }

        [Test]
        public void HasMatchingQuestionnaireId_should_return_true_for_different_guid_formats()
        {
            var id = Guid.NewGuid();
            var user = PrincipalWith(new Claim(JwtTokenService.QuestionnaireIdClaimType, id.ToString("N")));

            user.HasMatchingQuestionnaireId(id).Should().BeTrue();
        }

        [Test]
        public void HasMatchingQuestionnaireId_should_return_false_when_claim_differs()
        {
            var user = PrincipalWith(new Claim(JwtTokenService.QuestionnaireIdClaimType, Guid.NewGuid().ToString()));

            user.HasMatchingQuestionnaireId(Guid.NewGuid()).Should().BeFalse();
        }

        [Test]
        public void HasMatchingQuestionnaireId_should_return_false_when_claim_missing()
        {
            var user = PrincipalWith(new Claim("other", Guid.NewGuid().ToString()));

            user.HasMatchingQuestionnaireId(Guid.NewGuid()).Should().BeFalse();
        }

        [Test]
        public void HasMatchingQuestionnaireId_should_return_false_when_claim_is_not_a_guid()
        {
            var user = PrincipalWith(new Claim(JwtTokenService.QuestionnaireIdClaimType, "not-a-guid"));

            user.HasMatchingQuestionnaireId(Guid.NewGuid()).Should().BeFalse();
        }

        [Test]
        public void HasMatchingQuestionnaireId_should_return_false_for_anonymous_principal()
        {
            new ClaimsPrincipal(new ClaimsIdentity()).HasMatchingQuestionnaireId(Guid.NewGuid()).Should().BeFalse();
        }
    }
}

