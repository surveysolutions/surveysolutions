using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Main.Core.Documents;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Tests.Unit
{
    internal static class Setup
    {
        public static void InstanceToMockedServiceLocator<TInstance>(TInstance instance)
        {
            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<TInstance>())
                .Returns(instance);
        }

        public static void StubToMockedServiceLocator<T>()
            where T : class
        {
            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<T>())
                .Returns(Mock.Of<T>());
        }

        public static IReadSideKeyValueStorage<TEntity> ReadSideKeyValueStorageWithSameEntityForAnyGet<TEntity>(TEntity entity) where TEntity : class, IReadSideRepositoryEntity
            => Mock.Of<IReadSideKeyValueStorage<TEntity>>(_ => _.GetById(It.IsAny<string>()) == entity);

        public static IQueryableReadSideRepositoryReader<TEntity> QueryableReadSideRepositoryReaderByQueryResultType<TEntity, TResult>(IEnumerable<TEntity> entities)
            where TEntity : class, IReadSideRepositoryEntity
        {
            var repositoryReader = Mock.Of<IQueryableReadSideRepositoryReader<TEntity>>();

            Mock.Get(repositoryReader)
                .Setup(reader => reader.Query(It.IsAny<Func<IQueryable<TEntity>, TResult>>()))
                .Returns<Func<IQueryable<TEntity>, TResult>>(query => query.Invoke(entities.AsQueryable()));

            return repositoryReader;
        }

        public static void SelfCloningInterviewExpressionStateStubWithProviderToMockedServiceLocator(
            Guid questionnaireId, Expression<Func<IInterviewExpressionState, bool>> expressionStateMoqPredicate)
        {
            var expressionState = Mock.Of<IInterviewExpressionState>(expressionStateMoqPredicate);

            Mock.Get(expressionState).Setup(_ => _.Clone()).Returns(() => expressionState);

            var interviewExpressionStatePrototypeProvider = Mock.Of<IInterviewExpressionStatePrototypeProvider>(_
                => _.GetExpressionState(questionnaireId, It.IsAny<long>()) == expressionState);

            Setup.InstanceToMockedServiceLocator<IInterviewExpressionStatePrototypeProvider>(interviewExpressionStatePrototypeProvider);
        }

        public static IPlainQuestionnaireRepository QuestionnaireRepositoryWithOneQuestionnaire(Guid questionnaireId, Expression<Func<IQuestionnaire, bool>> questionnaireMoqPredicate)
        {
            var questionnaire = Mock.Of<IQuestionnaire>(questionnaireMoqPredicate);

            return Create.QuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);
        }

        public static IPlainQuestionnaireRepository QuestionnaireRepositoryWithOneQuestionnaire(
            QuestionnaireIdentity questionnaireIdentity, Expression<Func<IQuestionnaire, bool>> questionnaireMoqPredicate)
        {
            var questionnaire = Mock.Of<IQuestionnaire>(questionnaireMoqPredicate);

            return Mock.Of<IPlainQuestionnaireRepository>(repository
                => repository.GetQuestionnaire(questionnaireIdentity) == questionnaire
                && repository.GetHistoricalQuestionnaire(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version) == questionnaire);
        }

        public static IEventHandler FailingFunctionalEventHandler()
        {
            return FailingFunctionalEventHandlerHavingUniqueType<object>();
        }

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

        public static IEventHandler FailingOldSchoolEventHandler()
        {
            return FailingOldSchoolEventHandlerHavingUniqueType<object>();
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

        public static IStatefulInterviewRepository StatefulInterviewRepositoryWithInterviewsWithAllGroupsEnabledAndExisting()
        {
            return Setup.StatefulInterviewRepository(
                Mock.Of<IStatefulInterview>(_
                    => _.HasGroup(It.IsAny<Identity>()) == true
                    && _.IsEnabled(It.IsAny<Identity>()) == true));
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

            IPlainQuestionnaireRepository questionnaireRepository = Create.QuestionnaireRepositoryStubWithOneQuestionnaire(
                questionnaireId: questionnaireId,
                questionnaireVersion: questionnaireVersion,
                questionnaire: questionnaire);

            Interview interview = Create.Interview(questionnaireRepository: questionnaireRepository);

            interview.Apply(Create.Event.InterviewCreated(
                questionnaireId: questionnaireId,
                questionnaireVersion: questionnaireVersion));

            return interview;
        }

        public static Interview InterviewForQuestionnaireDocument(QuestionnaireDocument questionnaireDocument)
        {
            return Setup.InterviewForQuestionnaire(Create.PlainQuestionnaire(document: questionnaireDocument));
        }

        public static IDesignerEngineVersionService DesignerEngineVersionService(bool isClientVersionSupported = true, bool isQuestionnaireVersionSupported = true, int questionnaireContentVersion = 9)
        {
            var version = new Version(questionnaireContentVersion, 0, 0);

            return Mock.Of<IDesignerEngineVersionService>(_ 
                => _.IsClientVersionSupported(Moq.It.IsAny<Version>()) == isClientVersionSupported
                && _.IsQuestionnaireDocumentSupportedByClientVersion(Moq.It.IsAny<QuestionnaireDocument>(), Moq.It.IsAny<Version>()) == isQuestionnaireVersionSupported
                && _.GetQuestionnaireContentVersion(Moq.It.IsAny<QuestionnaireDocument>()) == version);
        }
    }
}