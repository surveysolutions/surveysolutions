using System;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Implementation;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.Infrastructure.Implementation;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Invitations
{
    [TestOf(typeof(TokenGenerator))]
    public class TokenGeneratorTests
    {
        [Test]
        public void When_generating_unique_one_letter_token()
        {
            var invitationStorage = new InMemoryPlainStorageAccessor<Invitation>();
            for (int i = 0; i < TokenGenerator.Encode_32_Chars.Length-1; i++)
            {
                var invitation = Create.Entity.Invitation(i + 1, Create.Entity.Assignment(), TokenGenerator.Encode_32_Chars.Substring(i, 1));
                invitationStorage.Store(invitation, invitation.Id);
            }

            var generator = Create.Service.TokenGenerator(1, invitationStorage: invitationStorage);

            var token = generator.GenerateUnique();

            Assert.That(token, Is.EqualTo("Z"));
        }

        public enum Component
        {
            QuestionnaireId,
            Version,
            TenantId
        }

        [TestCase(Component.QuestionnaireId)]
        [TestCase(Component.Version)]
        [TestCase(Component.TenantId)]
        public void should_generate_unique_uniform_tokens_for_different_component(Component component)
        {
            (Guid q, long v, Guid t) NewTestCaseData(int iteration)
            {
                var q = Guid.NewGuid();
                var version = rnd.Next(0, 100);
                var t = Guid.NewGuid();

                return component switch
                {
                    Component.QuestionnaireId => (Guid.NewGuid(), version, t),
                    Component.Version => (q, iteration, t),
                    Component.TenantId => (q, version, Guid.NewGuid()),
                    _ => throw new ArgumentOutOfRangeException(nameof(component), component, null)
                };
            }

            var listForSameQuestionnaireAndTenantButDifferentVersion = new List<string>();
            
            const int listlen = 20;

            for (int i = 1; i <= listlen; i++)
            {
                var testCase = NewTestCaseData(i);
                
                var token = GenerateToken(Create.Entity.QuestionnaireIdentity(testCase.q, testCase.v), testCase.t);
                listForSameQuestionnaireAndTenantButDifferentVersion.Add(token);
            }

            Assert.That(listForSameQuestionnaireAndTenantButDifferentVersion.Distinct().Count(), Is.EqualTo(listlen));
        }

        string GenerateToken(QuestionnaireIdentity questionnaire, Guid tenantId)
        {
            var tenantSettings = new TestPlainStorage<ServerSettings>();
            tenantSettings.Store(new ServerSettings
                {
                    Value = tenantId.ToString(),
                    Id = ServerSettings.PublicTenantIdKey
                },
                ServerSettings.PublicTenantIdKey);

            var generator = Create.Service.TokenGenerator(8, tenantSettingsStorage: tenantSettings);
            var token = generator.Generate(questionnaire);
            Console.WriteLine(token);
            return token;
        }

        Random rnd = new Random();

        QuestionnaireIdentity RandomQuestionnaireIdentity()
            => Create.Entity.QuestionnaireIdentity(questionnaireVersion: rnd.Next(1, 100));

    }
}
