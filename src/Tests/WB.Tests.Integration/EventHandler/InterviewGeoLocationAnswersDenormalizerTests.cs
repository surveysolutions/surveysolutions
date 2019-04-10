using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Mappings;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Tests.Abc;
using WB.Tests.Integration.PostgreSQLEventStoreTests;

namespace WB.Tests.Integration.EventHandler
{
    [TestOf(typeof(InterviewGeoLocationAnswersDenormalizer))]
    internal class InterviewGeoLocationAnswersDenormalizerTests
    {
        [Test]
        public void when_double_update_by_geo_question_answered_event_should_second_one_not_throw_an_exception()
        {
            // arrange
            var @event = Create.PublishedEvent.GeoLocationQuestionAnswered();
            var connectionString = DatabaseTestInitializer.InitializeDb(DbType.ReadSide);

            var sessionFactory = IntegrationCreate.SessionFactory(connectionString,
                new List<Type> {typeof(InterviewGpsMap)}, true, new UnitOfWorkConnectionSettings().ReadSideSchemaName);

            var unitOfWork = IntegrationCreate.UnitOfWork(sessionFactory);
            var denormalizer = InterviewGeoLocationAnswersDenormalizer(sessionProvider: unitOfWork);

            denormalizer.Update(null, @event);
            // act 
            // assert
            Assert.DoesNotThrow(() => denormalizer.Update(null, @event));
        }

        [Test]
        public void when_update_by_geo_question_answered_event_with_zero_timestamp_should_not_throw_an_exception()
        {
            // arrange
            var @event = Create.PublishedEvent.GeoLocationQuestionAnswered(timestamp: DateTimeOffset.MinValue);
            var connectionString = DatabaseTestInitializer.InitializeDb(DbType.ReadSide);

            var sessionFactory = IntegrationCreate.SessionFactory(connectionString,
                new List<Type> { typeof(InterviewGpsMap) }, true, new UnitOfWorkConnectionSettings().ReadSideSchemaName);

            var unitOfWork = IntegrationCreate.UnitOfWork(sessionFactory);
            var denormalizer = InterviewGeoLocationAnswersDenormalizer(sessionProvider: unitOfWork);

            denormalizer.Update(null, @event);
            // act 
            // assert
            Assert.DoesNotThrow(() => denormalizer.Update(null, @event));
        }

        private static InterviewGeoLocationAnswersDenormalizer InterviewGeoLocationAnswersDenormalizer(
            IUnitOfWork sessionProvider = null, IQuestionnaireStorage questionnaireStorage = null)
            => new InterviewGeoLocationAnswersDenormalizer(sessionProvider ?? Mock.Of<IUnitOfWork>(),
                questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>());
    }
}
