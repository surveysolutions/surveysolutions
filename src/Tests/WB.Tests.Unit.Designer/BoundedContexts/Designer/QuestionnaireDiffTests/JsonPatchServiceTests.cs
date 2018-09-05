using Newtonsoft.Json;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireDiffTests
{
    [TestOf(typeof(JsonPatchService))]
    public class JsonPatchServiceTests
    {
        readonly string TestJson = JsonConvert.SerializeObject(new
        {
            Foo = "foo"
        });

        readonly string TestJson1 = JsonConvert.SerializeObject(new
        {
            Bar = "bar"
        });

        [Test]
        public void Should_be_able_to_diff_jsons()
        {
            var jsonPatchService = CreateService();

            var diff = jsonPatchService.Diff(TestJson, TestJson1);

            var result = jsonPatchService.Apply(TestJson, diff);

            var deserializeObject = JsonConvert.DeserializeObject<dynamic>(result);
            Assert.That((string)deserializeObject.Bar, Is.EqualTo("bar"));
        }

        [Test]
        public void should_be_able_to_compare_with_null()
        {
            var jsonPatchService = CreateService();

            var diff = jsonPatchService.Diff(null, TestJson);

            Assert.That(diff, Is.Not.Empty);

            var patched = jsonPatchService.Apply(null, diff);

            var deserializeObject = JsonConvert.DeserializeObject<dynamic>(patched);
            Assert.That((string)deserializeObject.Foo, Is.EqualTo("foo"));
        }

        [Test]
        public void should_return_null_for_equal_json()
        {
            var jsonPatchService = CreateService();
            var diff = jsonPatchService.Diff(TestJson, TestJson);

            Assert.That(diff, Is.Null);
        }

        [Test]
        public void should_no_try_applying_null_diff()
        {
            var jsonPatchService = CreateService();

            string result = jsonPatchService.Apply(TestJson, null);
            Assert.That(result, Is.EqualTo(TestJson));
        }

        [Test]
        public void should_be_able_to_compare_with_right_null()
        {
            var jsonPatchService = CreateService();

            var diff = jsonPatchService.Diff(TestJson, null);

            Assert.That(diff, Is.Not.Empty);

            var patched = jsonPatchService.Apply(TestJson, diff);

            Assert.That(patched, Is.Null);
        }

        private JsonPatchService CreateService()
        {
            return new JsonPatchService();
        }
    }
}
