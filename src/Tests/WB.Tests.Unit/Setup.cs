﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Providers;
using WB.Core.SharedKernels.DataCollection.Repositories;
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

        public static void SimpleQuestionnaireDocumentUpgraderToMockedServiceLocator()
        {
            var questionnaireDocumentUpgrader = Mock.Of<IQuestionnaireDocumentUpgrader>();

            Setup.InstanceToMockedServiceLocator(questionnaireDocumentUpgrader);
        }

        public static IQueryableReadSideRepositoryReader<TEntity> QueryableReadSideRepositoryReaderByQueryResultType<TEntity, TResult>(IEnumerable<TEntity> entities)
            where TEntity : class, IReadSideRepositoryEntity
        {
            var repositoryReader = Mock.Of<IQueryableReadSideRepositoryReader<TEntity>>();

            Mock.Get(repositoryReader)
                .Setup(reader => reader.Query(It.IsAny<Func<IQueryable<TEntity>, TResult>>()))
                .Returns<Func<IQueryable<TEntity>, TResult>>(query => query.Invoke(entities.AsQueryable()));

            Mock.Get(repositoryReader)
                .Setup(reader => reader.QueryAll(It.IsAny<Expression<Func<TEntity, bool>>>()))
                .Returns<Expression<Func<TEntity, bool>>>(condition =>
                {
                    Func<TEntity, bool> predicate = condition.Compile();
                    return entities.Where(predicate);
                });

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

        public static void QuestionnaireWithRepositoryToMockedServiceLocator(
            Guid questionnaireId, Expression<Func<IQuestionnaire, bool>> questionnaireMoqPredicate)
        {
            var questionnaire = Mock.Of<IQuestionnaire>(questionnaireMoqPredicate);

            Setup.InstanceToMockedServiceLocator<IQuestionnaireRepository>(
                Create.QuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire));
        }
    }
}