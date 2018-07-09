using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Supervisor.Services.Implementation;
using WB.Core.BoundedContexts.Supervisor.Services.Implementation.OfflineSyncHandlers;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Supervisor.Services
{
    [TestOf(typeof(SupervisorQuestionnairesHandler))]
    public class SupervisorQuestionnairesHandlerTests
    {
        private Fixture fixture;

        [SetUp]
        public void Setup()
        {
            this.fixture = Create.Other.AutoFixture();
        }

        [Test]
        public async Task GetQuestionnaireList_call_GetAllQuestionnaireIdentities()
        {
            var accessor = fixture.Freeze<Mock<IInterviewerQuestionnaireAccessor>>();
            var handler = fixture.Create<SupervisorQuestionnairesHandler>();

            //act
            await handler.GetList(new GetQuestionnaireList.Request());

            //assert
            accessor.Verify(s => s.GetAllQuestionnaireIdentities(), Times.Once);
        }

        [Test]
        public async Task GetQuestionnaireTranslations_should_get_only_for_specified_questionnaire()
        {
            var questionnaireId = Create.Entity.QuestionnaireIdentity(Id.g1, 42);
            var otherQuestionnaireid = Create.Entity.QuestionnaireIdentity(Id.g2);
            var translations = new InMemoryPlainStorage<TranslationInstance>();

            translations.Store(new List<TranslationInstance>
            {
                Create.Entity.TranslationInstance_Enumetaror(questionnaireId: questionnaireId.Id, value: "1"),
                Create.Entity.TranslationInstance_Enumetaror(questionnaireId: questionnaireId.Id, value: "2"),
                Create.Entity.TranslationInstance_Enumetaror(questionnaireId: otherQuestionnaireid.Id, value: "3")
            });

            fixture.Register<IPlainStorage<TranslationInstance>>(() => translations);

            var handler = fixture.Create<SupervisorQuestionnairesHandler>();

            var response = await handler.GetQuestionnaireTranslation(new GetQuestionnaireTranslationRequest
            {
                QuestionnaireIdentity = questionnaireId
            });
          
            Assert.That(response.Translations, Has.Count.EqualTo(2));
            Assert.That(response.Translations, Has.None.Property(nameof(TranslationInstance.Value)).EqualTo("3"));
        }
    }
}
