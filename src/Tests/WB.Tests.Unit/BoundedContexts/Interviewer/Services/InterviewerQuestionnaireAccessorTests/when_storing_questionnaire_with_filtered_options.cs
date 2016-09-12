using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using Nito.AsyncEx.Synchronous;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Questionnaire.Translations;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.InterviewerQuestionnaireAccessorTests
{
    internal class when_storing_questionnaire_with_filtered_options : InterviewerQuestionnaireAccessorTestsContext
    {
        Establish context = () =>
        {

            var questionnaireDocument = new QuestionnaireDocument()
            {
                Title = "title of questionnaire",
                Children = new List<IComposite>()
                {
                    new SingleQuestion()
                    {
                        PublicKey = questionId,
                        IsFilteredCombobox = true,
                        Answers = new List<Answer>()
                        {
                            new Answer()
                            {
                                AnswerCode = 1, AnswerText = "test", AnswerValue = "1"
                            }
                        }
                    }
                }
            };

            var synchronizationSerializer = Mock.Of<IJsonAllTypesSerializer>(x => x.Deserialize<QuestionnaireDocument>(Moq.It.IsAny<string>()) == questionnaireDocument);
            interviewerQuestionnaireAccessor = CreateInterviewerQuestionnaireAccessor(
                synchronizationSerializer: synchronizationSerializer,
                questionnaireViewRepository: mockOfQuestionnaireViewRepository.Object,
                questionnaireStorage: mockOfPlainQuestionnaireRepository.Object,
                optionsRepository : mockOfOptionsRepositoryRepository.Object,
                translationRepository: mockOfTranslationRepository.Object);
        };

        Because of = () => interviewerQuestionnaireAccessor.StoreQuestionnaire(questionnaireIdentity, questionnaireDocumentAsString, isCensusQuestionnaire, new List<TranslationDto>());

        It should_store_questionnaire_document_view_to_plain_storage = () =>
            mockOfPlainQuestionnaireRepository.Verify(x => x.StoreQuestionnaire(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version, Moq.It.IsAny<QuestionnaireDocument>()), Times.Once);

        It should_store_questionnaire_view_to_plain_storage = () =>
            mockOfQuestionnaireViewRepository.Verify(x => x.Store(Moq.It.IsAny<QuestionnaireView>()), Times.Once);

        It should_store_translations_to_storage = () =>
            mockOfTranslationRepository.Verify(x => x.Store(Moq.It.IsAny<List<TranslationInstance>>()), Times.Once);

        It should_remove_options_to_storage = () =>
            mockOfOptionsRepositoryRepository.Verify(x => x.RemoveOptionsForQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>()), Times.Once);

        It should_store_options_to_storage = () =>
            mockOfOptionsRepositoryRepository.Verify(x => x.StoreOptionsForQuestion(Moq.It.IsAny<QuestionnaireIdentity>(), questionId, Moq.It.IsAny <List<Answer>>(), Moq.It.IsAny<List<TranslationDto>>()), Times.Once);

        private static readonly QuestionnaireIdentity questionnaireIdentity = new QuestionnaireIdentity(Guid.Parse("11111111111111111111111111111111"), 1);
        private static readonly Mock<IQuestionnaireStorage> mockOfPlainQuestionnaireRepository = new Mock<IQuestionnaireStorage>();
        private static readonly Mock<IAsyncPlainStorage<QuestionnaireView>> mockOfQuestionnaireViewRepository = new Mock<IAsyncPlainStorage<QuestionnaireView>>();
        private const bool isCensusQuestionnaire = true;
        private const string questionnaireDocumentAsString = "questionnaire document";
        private static Guid questionId = Guid.Parse("22222222222222222222222222222222");
        private static InterviewerQuestionnaireAccessor interviewerQuestionnaireAccessor;


        private static readonly Mock<IAsyncPlainStorage<TranslationInstance>> mockOfTranslationRepository = new Mock<IAsyncPlainStorage<TranslationInstance>>();
        private static readonly Mock<IOptionsRepository> mockOfOptionsRepositoryRepository = new Mock<IOptionsRepository>();
        

    }
}
