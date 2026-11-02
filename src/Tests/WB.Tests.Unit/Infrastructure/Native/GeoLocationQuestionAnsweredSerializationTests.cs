using System;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Infrastructure.Native.Storage;

namespace WB.Tests.Unit.Infrastructure.Native
{
    [TestOf(typeof(GeoLocationQuestionAnswered))]
    public class GeoLocationQuestionAnsweredSerializationTests
    {
        private static GeoLocationQuestionAnswered CreateEvent() =>
            new GeoLocationQuestionAnswered(Guid.NewGuid(), Guid.NewGuid(), Array.Empty<decimal>(),
                DateTimeOffset.UtcNow, latitude: -1.234, longitude: 1.00025, accuracy: 10, altitude: 34,
                timestamp: DateTimeOffset.UtcNow, gpsProvider: "gps", isFromMockProvider: true);

        [Test]
        public void when_serialized_with_synchronization_settings_should_keep_gps_provider_and_mock_flag()
        {
            var @event = CreateEvent();

            var json = JsonConvert.SerializeObject(@event, EventSerializerSettings.SyncronizationJsonSerializerSettings);
            var restored = JsonConvert.DeserializeObject<GeoLocationQuestionAnswered>(json,
                EventSerializerSettings.SyncronizationJsonSerializerSettings);

            restored.GpsProvider.Should().Be("gps");
            restored.IsFromMockProvider.Should().BeTrue();
        }

        [Test]
        public void when_serialized_with_event_store_settings_should_keep_gps_provider_and_mock_flag()
        {
            var @event = CreateEvent();

            var json = JsonConvert.SerializeObject(@event, EventSerializerSettings.BackwardCompatibleJsonSerializerSettings);
            var restored = JsonConvert.DeserializeObject<GeoLocationQuestionAnswered>(json,
                EventSerializerSettings.BackwardCompatibleJsonSerializerSettings);

            restored.GpsProvider.Should().Be("gps");
            restored.IsFromMockProvider.Should().BeTrue();
        }

        [Test]
        public void when_serialized_by_all_types_serializer_should_keep_gps_provider_and_mock_flag()
        {
            var @event = CreateEvent();
            var serializer = new JsonAllTypesSerializer();

            var json = serializer.Serialize(@event);
            var restored = serializer.Deserialize<GeoLocationQuestionAnswered>(json);

            restored.GpsProvider.Should().Be("gps");
            restored.IsFromMockProvider.Should().BeTrue();
        }
    }
}
