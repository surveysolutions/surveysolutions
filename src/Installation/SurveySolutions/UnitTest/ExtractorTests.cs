using System.IO;
using Microsoft.Deployment.WindowsInstaller;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Moq;
using SurveySolutionsCustomActions;
using Assert = NUnit.Framework.Assert;

namespace UnitTests
{
    public class CustomActionsTests
    {
        [Test]
        public void WhenGettingSettingsFromOldConfig()
        {
            ISettingsExtractor extractor= new SettingsExtractor();

            var directory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var path = Path.Combine(directory, "test.config.xml");

            var config = extractor.GetConfig(path);
            Assert.That(config.BaseURL,Is.EqualTo("https://armbn.mysurvey.solutions"));
            Assert.That(config.TenantName, Is.EqualTo("armbn"));
            Assert.That(config.ConnectionString, Is.EqualTo("Persist Security Info=true;Server=prod2.db.mysurvey.solutions;Port=5432;User Id=armbn;Password=aaa;Database=armbn;ApplicationName=armbn"));
        }
    }
}
