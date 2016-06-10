﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Core.SharedKernels.QuestionnaireEntities;
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

            return Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireId, questionnaire);
        }

        public static IPlainQuestionnaireRepository QuestionnaireRepositoryWithOneQuestionnaire(
            QuestionnaireIdentity questionnaireIdentity, QuestionnaireDocument questionnaireDocument)
            => Setup.QuestionnaireRepositoryWithOneQuestionnaire(
                questionnaireIdentity,
                Create.Entity.PlainQuestionnaire(questionnaireDocument));

        public static IPlainQuestionnaireRepository QuestionnaireRepositoryWithOneQuestionnaire(
            QuestionnaireIdentity questionnaireIdentity, Expression<Func<IQuestionnaire, bool>> questionnaireMoqPredicate)
            => Setup.QuestionnaireRepositoryWithOneQuestionnaire(
                questionnaireIdentity,
                Mock.Of<IQuestionnaire>(questionnaireMoqPredicate));

        private static IPlainQuestionnaireRepository QuestionnaireRepositoryWithOneQuestionnaire(
            QuestionnaireIdentity questionnaireIdentity, IQuestionnaire questionnaire)
            => Mock.Of<IPlainQuestionnaireRepository>(repository
                => repository.GetQuestionnaire(questionnaireIdentity) == questionnaire
                && repository.GetHistoricalQuestionnaire(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version) == questionnaire);

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

            IPlainQuestionnaireRepository questionnaireRepository = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(
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
            var version = new Version(questionnaireContentVersion, 0, 0);

            return Mock.Of<IDesignerEngineVersionService>(_ 
                => _.IsClientVersionSupported(Moq.It.IsAny<Version>()) == isClientVersionSupported
                && _.IsQuestionnaireDocumentSupportedByClientVersion(Moq.It.IsAny<QuestionnaireDocument>(), Moq.It.IsAny<Version>()) == isQuestionnaireVersionSupported
                && _.GetQuestionnaireContentVersion(Moq.It.IsAny<QuestionnaireDocument>()) == version);
        }

        public static StatefulInterview StatefulInterview(QuestionnaireDocument questionnaireDocument)
        {
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity();

            var questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireIdentity, questionnaireDocument);

            return Create.AggregateRoot.StatefulInterview(
                questionnaireId: questionnaireIdentity.QuestionnaireId,
                questionnaireVersion: questionnaireIdentity.Version,
                questionnaireRepository: questionnaireRepository);
        }

        public static Mock<IQuestionnaireEntityFactory> QuestionnaireEntityFactoryWithStaticText(Guid? entityId = null, string text = null, string attachmentName = null)
        {
            var staticText = Create.Entity.StaticText(entityId, text, attachmentName);
            var questionnaireEntityFactoryMock = new Mock<IQuestionnaireEntityFactory>();
            if (!entityId.HasValue)
            {
                questionnaireEntityFactoryMock
                   .Setup(x => x.CreateStaticText(Moq.It.IsAny<Guid>(), Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<bool>(), Moq.It.IsAny<IList<ValidationCondition>>()))
                   .Returns((Guid id, string t, string a) => Create.Entity.StaticText(id, t, a));
            }
            else if (string.IsNullOrWhiteSpace(attachmentName))
            {
                questionnaireEntityFactoryMock
                    .Setup(x => x.CreateStaticText(entityId.Value, text, Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<bool>(), Moq.It.IsAny<IList<ValidationCondition>>()))
                    .Returns(staticText);
            }
            else
            {
                questionnaireEntityFactoryMock
                   .Setup(x => x.CreateStaticText(entityId.Value, text, attachmentName, Moq.It.IsAny<string>(), Moq.It.IsAny<bool>(), Moq.It.IsAny<IList<ValidationCondition>>()))
                   .Returns(staticText);
            }

            return questionnaireEntityFactoryMock;
        }

        public static ISupportedVersionProvider SupportedVersionProvider(Version supportedVerstion)
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

        public static IPrincipal InterviewerPrincipal(string name, string pass)
        {
            return Mock.Of<IPrincipal>(p => p.CurrentUserIdentity == new InterviewerIdentity() { Name = "name", Password = "pass" });
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
            filteredOptionsViewModel.Setup(x => x.Options).Returns(options);
            filteredOptionsViewModel.Setup(x => x.Init(It.IsAny<string>(), It.IsAny<Identity>()));

            return filteredOptionsViewModel.Object;
        }
    }
}