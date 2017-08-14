using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using WB.UI.Headquarters.Models;

namespace WB.Tests.Unit.Applications.Headquarters.Translation
{
    public class TranslationModelTests
    {
        [TestCase("en-US")]
        [TestCase("ru-RU")]
        public void CanAddTranslationFromResource(string langCode)
        {
            using (new ChangeCurrentCulture(CultureInfo.GetCultureInfo(langCode)))
            {
                var model = new TranslationModel();

                model.Add(UnitTestSample.ResourceManager);

                Assert.That(model.Object[nameof(UnitTestSample)][nameof(UnitTestSample.ResourceKey1)], Is.EqualTo(UnitTestSample.ResourceKey1));
                Assert.That(model.Object[nameof(UnitTestSample)][nameof(UnitTestSample.SampleKey)], Is.EqualTo(UnitTestSample.SampleKey));
            } 
        }

        [TestCase("en-US")]
        [TestCase("ru-RU")]
        public void CanAddTranslationFromSeveralResources(string langCode)
        {
            using (new ChangeCurrentCulture(CultureInfo.GetCultureInfo(langCode)))
            {
                var model = new TranslationModel();

                model.Add(UnitTestSample.ResourceManager, UnitTestOtherSample.ResourceManager);

                Assert.That(model.Object[nameof(UnitTestSample)][nameof(UnitTestSample.ResourceKey1)], Is.EqualTo(UnitTestSample.ResourceKey1));
                Assert.That(model.Object[nameof(UnitTestOtherSample)][nameof(UnitTestOtherSample.OtherKey1)], Is.EqualTo(UnitTestOtherSample.OtherKey1));
            }
        }

        [Test]
        public void ShouldSerializeToJsonPreservingResourceKeys()
        {
            using (new ChangeCurrentCulture(CultureInfo.GetCultureInfo("en-US")))
            {
                var model = new TranslationModel(UnitTestSample.ResourceManager, UnitTestOtherSample.ResourceManager);

                var jsonString = model.ToString();

                var json = JObject.Parse(jsonString);

                Assert.NotNull(json[nameof(UnitTestSample)], "Should move each resource as one of root namespaces");
                Assert.NotNull(json[nameof(UnitTestOtherSample)], "Should move each resource as one of root namespaces");

                Assert.That(json[nameof(UnitTestSample)][nameof(UnitTestSample.SampleKey)].Value<string>(), Is.EqualTo(UnitTestSample.SampleKey));
            }
        }
    }
}
