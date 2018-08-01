using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoFixture;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Abc
{
    internal static class Setup
    {
        public static void InstanceToMockedServiceLocator<TInstance>(TInstance instance)
        {
            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<TInstance>())
                .Returns(instance);
            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance(typeof (TInstance)))
                .Returns(instance);
        }

        public static void StubToMockedServiceLocator<T>()
            where T : class
        {
            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<T>())
                .Returns(Mock.Of<T>());
        }

        public static IQuestionnaireStorage QuestionnaireRepositoryWithOneQuestionnaire(Guid questionnaireId, Expression<Func<IQuestionnaire, bool>> questionnaireMoqPredicate)
        {
            var questionnaire = Mock.Of<IQuestionnaire>(questionnaireMoqPredicate);

            return Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireId, questionnaire);
        }

        public static IQuestionnaireStorage QuestionnaireRepositoryWithOneQuestionnaire(QuestionnaireDocument questionnaireDocument)
            => Setup.QuestionnaireRepositoryWithOneQuestionnaire(Create.Entity.PlainQuestionnaire(questionnaireDocument));

        public static IQuestionnaireStorage QuestionnaireRepositoryWithOneQuestionnaire(
            QuestionnaireIdentity questionnaireIdentity, QuestionnaireDocument questionnaireDocument)
            => Setup.QuestionnaireRepositoryWithOneQuestionnaire(Create.Entity.PlainQuestionnaire(questionnaireDocument));

        public static IQuestionnaireStorage QuestionnaireRepositoryWithOneQuestionnaire(
            QuestionnaireIdentity questionnaireIdentity, Expression<Func<IQuestionnaire, bool>> questionnaireMoqPredicate)
            => Setup.QuestionnaireRepositoryWithOneQuestionnaire(Mock.Of<IQuestionnaire>(questionnaireMoqPredicate));


        public static IQuestionnaireStorage QuestionnaireRepositoryWithOneQuestionnaire(IQuestionnaire questionnaire)
            => Stub<IQuestionnaireStorage>.Returning(questionnaire);

        public static IEventHandler FailingFunctionalEventHandlerHavingUniqueType<TUniqueType>()
        {
            var uniqueEventHandlerMock = new Mock<IEnumerable<TUniqueType>>();
            var eventHandlerMock = uniqueEventHandlerMock.As<IEventHandler>();
            var eventHandlerAsFunctional = eventHandlerMock.As<IFunctionalEventHandler>();
            eventHandlerAsFunctional
                .Setup(_ => _.Handle(It.IsAny<IEnumerable<IPublishableEvent>>(), It.IsAny<Guid>()))
                .Throws<Exception>();

            return eventHandlerMock.Object;
        }

        public static IEventHandler FailingOldSchoolEventHandlerHavingUniqueType<TUniqueType>()
        {
            var uniqueEventHandlerMock = new Mock<IEnumerable<TUniqueType>>();
            var eventHandlerMock = uniqueEventHandlerMock.As<IEventHandler>();
            var eventHandlerAsOldSchool = eventHandlerMock.As<IEventHandler<IEvent>>();
            eventHandlerAsOldSchool
                .Setup(_ => _.Handle(It.IsAny<IPublishedEvent<IEvent>>()))
                .Throws<Exception>();

            return eventHandlerMock.Object;
        }

        public static IStatefulInterviewRepository StatefulInterviewRepository(IStatefulInterview interview)
        {
            return Mock.Of<IStatefulInterviewRepository>(_
                => _.Get(It.IsAny<string>()) == interview);
        }

        public static Interview InterviewForQuestionnaire(IQuestionnaire questionnaire)
        {
            Guid questionnaireId = Guid.NewGuid();
            long questionnaireVersion = 777;

            IQuestionnaireStorage questionnaireRepository = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(
                questionnaireId: questionnaireId,
                questionnaireVersion: questionnaireVersion,
                questionnaire: questionnaire);

            Interview interview = Create.AggregateRoot.Interview(questionnaireRepository: questionnaireRepository);

            interview.Apply(Create.Event.InterviewCreated(
                questionnaireId: questionnaireId,
                questionnaireVersion: questionnaireVersion));

            return interview;
        }

        public static Interview InterviewForQuestionnaireDocument(QuestionnaireDocument questionnaireDocument)
        {
            return Setup.InterviewForQuestionnaire(Create.Entity.PlainQuestionnaire(document: questionnaireDocument));
        }

        public static IDesignerEngineVersionService DesignerEngineVersionService(bool isClientVersionSupported = true, bool isQuestionnaireVersionSupported = true, int questionnaireContentVersion = 9)
        {
            return Mock.Of<IDesignerEngineVersionService>(_ 
                => _.IsClientVersionSupported(Moq.It.IsAny<int>()) == isClientVersionSupported
                && _.GetQuestionnaireContentVersion(Moq.It.IsAny<QuestionnaireDocument>()) == questionnaireContentVersion);
        }

        public static StatefulInterview StatefulInterview(QuestionnaireDocument questionnaireDocument, bool census = true)
        {
            questionnaireDocument.IsUsingExpressionStorage = true;
            var readOnlyQuestionnaireDocument = questionnaireDocument.AsReadOnly();

            var expressionsPlayOrderProvider = Create.Service.ExpressionsPlayOrderProvider();
            questionnaireDocument.ExpressionsPlayOrder = expressionsPlayOrderProvider.GetExpressionsPlayOrder(readOnlyQuestionnaireDocument); 
            questionnaireDocument.DependencyGraph = expressionsPlayOrderProvider.GetDependencyGraph(readOnlyQuestionnaireDocument); 
            questionnaireDocument.ValidationDependencyGraph = expressionsPlayOrderProvider.GetValidationDependencyGraph(readOnlyQuestionnaireDocument); 

            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity();

            var questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireIdentity, questionnaireDocument);

            return Create.AggregateRoot.StatefulInterview(
                questionnaireId: questionnaireIdentity.QuestionnaireId,
                questionnaireVersion: questionnaireIdentity.Version,
                questionnaireRepository: questionnaireRepository,
                shouldBeInitialized: census);
        }

        public static ISupportedVersionProvider SupportedVersionProvider(int supportedVerstion)
        {
            var versionProvider = new Mock<ISupportedVersionProvider>();
            versionProvider.Setup(x => x.GetSupportedQuestionnaireVersion()).Returns(supportedVerstion);

            return versionProvider.Object;
        }

        public static IStringCompressor StringCompressor_Decompress<TEntity>(TEntity entity) where TEntity: class
        {
            var zipUtilsMock = new Mock<IStringCompressor>();

            zipUtilsMock.Setup(_ => _.DecompressString<TEntity>(Moq.It.IsAny<string>()))
                .Returns(entity);

            return zipUtilsMock.Object;
        }

        public static IInterviewerPrincipal InterviewerPrincipal(string name, string pass)
        {
            var interviewerIdentity = new InterviewerIdentity() {Name = "name", PasswordHash = "pass"};
            return InterviewerPrincipal(interviewerIdentity);
        }

        public static IInterviewerPrincipal InterviewerPrincipal(IInterviewerUserIdentity interviewerIdentity)
        {
            var interviewerPrincipal = new Mock<IInterviewerPrincipal>();
            interviewerPrincipal.Setup(x => x.CurrentUserIdentity).Returns(interviewerIdentity);
            interviewerPrincipal.As<IPrincipal>().Setup(x => x.CurrentUserIdentity).Returns(interviewerIdentity);
            return interviewerPrincipal.Object;
        }

        public static IPlainStorageAccessor<TEntity> PlainStorageAccessorWithOneEntity<TEntity>(object id, TEntity entity)
            where TEntity : class
            => new TestPlainStorage<TEntity>(new Dictionary<object, TEntity>
            {
                { id, entity },
            });

        public static FilteredOptionsViewModel FilteredOptionsViewModel(List<CategoricalOption> optionList = null)
        {
            var options = optionList ?? new List<CategoricalOption>
            {
                Create.Entity.CategoricalQuestionOption(1, "abc"),
                Create.Entity.CategoricalQuestionOption(2, "bbc"),
                Create.Entity.CategoricalQuestionOption(3, "bbc"),
                Create.Entity.CategoricalQuestionOption(4, "bbaé"),
                Create.Entity.CategoricalQuestionOption(5, "cccé"),
            };

            Mock<FilteredOptionsViewModel> filteredOptionsViewModel = new Mock<FilteredOptionsViewModel>();
            filteredOptionsViewModel.Setup(x => x.GetOptions(It.IsAny<string>())).Returns<string>(filter=>options.FindAll(x=>x.Title.Contains(filter)));
            filteredOptionsViewModel.Setup(x => x.Init(It.IsAny<string>(), It.IsAny<Identity>(), It.IsAny<int>()));

            return filteredOptionsViewModel.Object;
        }

        internal static void ApplyInterviewEventsToViewModels(IEventSourcedAggregateRoot interview, ILiteEventRegistry eventRegistry, Guid interviewId)
        {
            foreach (var evnt in interview.GetUnCommittedChanges().Select(x => Create.Other.CommittedEvent(x, interviewId)))
            {
                foreach (var handler in eventRegistry.GetHandlers(evnt))
                {
                    handler.Invoke(evnt);
                }
            }
        }

        internal static StatefulInterview StatefulInterviewWithMultilanguageQuestionnaires(
            params KeyValuePair<string, IComposite[]>[] questionnaires)
        {
            var chapterId = Guid.Parse("33333333333333333333333333333333");

            var questionnaireDocuments = new List<KeyValuePair<string, QuestionnaireDocument>>();

            foreach (var questionnaire in questionnaires)
            {
                var questionnaireDocumentWithOneChapterAndLanguages = Create.Entity.QuestionnaireDocumentWithOneChapterAndLanguages(
                        chapterId,
                        questionnaires.Select(x => x.Key).Where(x => x != null).ToArray(),
                        questionnaire.Value);

                questionnaireDocuments.Add(new KeyValuePair<string, QuestionnaireDocument>(questionnaire.Key, questionnaireDocumentWithOneChapterAndLanguages));
            }

            var questionnaireRepository = Create.Fake.QuestionnaireRepository(questionnaireDocuments.ToArray());

            return Create.AggregateRoot.StatefulInterview(questionnaireRepository: questionnaireRepository);
        }

        public static Mock<T> GetMock<T>(this IFixture fixture) where T : class
        {
            return fixture.Freeze<Mock<T>>();
        }

        public static IPrincipal Principal(string name, string pass)
        {
            return Mock.Of<IPrincipal>(p => p.CurrentUserIdentity == Mock.Of<IUserIdentity>(i => i.Name == "name" && i.PasswordHash == "pass"));
        }

        public static IPlainStorageAccessor<QuestionnaireBrowseItem> QuestionnaireBrowseItemRepository(params QuestionnaireBrowseItem[] questionnaireBrowseItem)
        {
            var questionnaireBrowseItemStorage = Mock.Of<IPlainStorageAccessor<QuestionnaireBrowseItem>>();

            Mock.Get(questionnaireBrowseItemStorage)
                .Setup(reader => reader.Query(It.IsAny<Func<IQueryable<QuestionnaireBrowseItem>, List<QuestionnaireBrowseItem>>>()))
                .Returns<Func<IQueryable<QuestionnaireBrowseItem>, List<QuestionnaireBrowseItem>>>(query => query.Invoke(questionnaireBrowseItem.AsQueryable()));

            return questionnaireBrowseItemStorage;
        }
    }
}
