using Machine.Specifications;
using System.Collections.Generic;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Portable;
using System;
using Newtonsoft.Json;
using WB.Core.Infrastructure.EventBus;
using WB.Infrastructure.Native.Storage;

namespace WB.Tests.Unit.GenericSubdomains.Utils.NewtonJsonUtilsTests
{
    internal class when_deserializing_designer_historical_events_Json 
    {
        private class AccountRegistered : IEvent
        {
            public string ApplicationName { get; set; }
            public string ConfirmationToken { get; set; }
            public string Email { get; set; }
        }

        Establish context = () =>
        {
            eventTypeResolver = new EventTypeResolver();
            eventTypeResolver.RegisterEventDataType(typeof(AccountRegistered));
            
            jsonSerializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                NullValueHandling = NullValueHandling.Ignore,
                FloatParseHandling = FloatParseHandling.Decimal,
                Formatting = Formatting.None,
                Binder = new OldToNewAssemblyRedirectSerializationBinder()
            };

            data.Add(new Tuple<string,string>(
                "accountRegistered",
                "{\r\n  \"userName\": \"tester\",\r\n  \"email\": \"v@example.com\",\r\n  \"confirmationToken\": \"XSB3u0UErLke9nm6JaQ6Kg2\",\r\n  \"createdDate\": \"2015-12-10T21:52:38.0888075Z\"\r\n}"
                ));
        };

        Because of = () =>
            data.ForEach(x => result.Add(JsonConvert.DeserializeObject(x.Item2, eventTypeResolver.ResolveType(x.Item1.ToPascalCase()), jsonSerializerSettings)));

        It should_return_not_null_result = () =>
            result.Count.ShouldEqual(1);

        private static JsonSerializerSettings jsonSerializerSettings;

        private static EventTypeResolver eventTypeResolver;
        private static List<Tuple<string, string>> data = new List<Tuple<string, string>>();
        private static HashSet<object> result = new HashSet<object>();

        
    }
}