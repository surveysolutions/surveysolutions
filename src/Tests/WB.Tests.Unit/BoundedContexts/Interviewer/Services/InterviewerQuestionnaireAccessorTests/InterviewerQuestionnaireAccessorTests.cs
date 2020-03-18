using System.Collections.Generic;
using Main.Core.Documents;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveySolutions.ReusableCategories;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.InterviewerQuestionnaireAccessorTests
{
    [TestFixture]
    internal class InterviewerQuestionnaireAccessorTests : InterviewerQuestionnaireAccessorTestsContext
    {
        [Test]
        public void when_store_questionnaire_with_reusable_categories()
        {
            var questionnaireIdentity = Id.QuestionnaireIdentity1;

            var plainStorage = Create.Storage.SqliteInmemoryStorage<OptionView, int?>();
            var optionsRepository = Create.Storage.OptionsRepository(plainStorage);

            var categoriesDtos = new List<ReusableCategoriesDto>()
            {
                Create.Entity.ReusableCategoriesDto(Id.g1, count: 10),
                Create.Entity.ReusableCategoriesDto(Id.g2, count: 15001),
                Create.Entity.ReusableCategoriesDto(Id.g3, count: 7),
            };
            var questionnaireDocument = Create.Entity.QuestionnaireDocument();

            var synchronizationSerializer = Mock.Of<IJsonAllTypesSerializer>(x => x.Deserialize<QuestionnaireDocument>(Moq.It.IsAny<string>()) == questionnaireDocument);
            var interviewerQuestionnaireAccessor = CreateInterviewerQuestionnaireAccessor(
                synchronizationSerializer: synchronizationSerializer,
                questionnaireViewRepository: Mock.Of<IPlainStorage<QuestionnaireView>>(),
                questionnaireStorage: Mock.Of<IQuestionnaireStorage>(),
                optionsRepository: optionsRepository,
                translationRepository: Mock.Of<IPlainStorage<TranslationInstance>>());

            interviewerQuestionnaireAccessor.StoreQuestionnaire(questionnaireIdentity, "questionnaire document", false, new List<TranslationDto>(), 
                categoriesDtos);

            var options1 = optionsRepository.GetReusableCategoriesById(questionnaireIdentity, Id.g1);
            Assert.That(options1.Length, Is.EqualTo(10));

            var options2 = optionsRepository.GetReusableCategoriesById(questionnaireIdentity, Id.g2);
            Assert.That(options2.Length, Is.EqualTo(15001));

            var options3 = optionsRepository.GetReusableCategoriesById(questionnaireIdentity, Id.g3);
            Assert.That(options3.Length, Is.EqualTo(7));
        }
    }
}
