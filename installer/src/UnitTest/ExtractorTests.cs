using System.IO;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
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

        [Test]
        public void CanUpdateBundlePathConfig()
        {
            File.WriteAllText("sample.xml", @"<?xml version=""1.0"" encoding=""utf-8""?>
<!--
  For more information on how to configure your ASP.NET application, please visit
  http://go.microsoft.com/fwlink/?LinkId=169433
  -->
<configuration>
  <system.web>
    <httpRuntime maxRequestLength=""307200"" maxQueryStringLength=""10240"" />
  </system.web>
  <system.webServer>
    <security>
      <requestFiltering>
        <requestLimits maxAllowedContentLength=""314572800"" maxUrl=""10241"" maxQueryString=""20481"" />
      </requestFiltering>
    </security>
    <handlers>
      <add name=""aspNetCore"" path=""*"" verb=""*"" modules=""AspNetCoreModuleV2"" resourceType=""Unspecified"" />
    </handlers>
    <aspNetCore processPath=""%LAUNCHER_PATH%"" stdoutLogEnabled=""false"" stdoutLogFile="".\logs\stdout"" arguments=""%LAUNCHER_ARGS%"">
      <environmentVariables>
        <environmentVariable name=""DOTNET_BUNDLE_EXTRACT_BASE_DIR"" value="".\.net-app"" />
      </environmentVariables>
    </aspNetCore>
  </system.webServer>
</configuration>");

            var file = new FileInfo("sample.xml");

            CustomActions.UpdateWebConfigContent(file.FullName);
            
            var xml = File.ReadAllText(file.FullName);
            var xdoc = XDocument.Parse(xml).Descendants("environmentVariable")
                .Single(x => x.Attribute("name")?.Value == "DOTNET_BUNDLE_EXTRACT_BASE_DIR");

            Assert.That(xdoc.Attribute("value").Value, Is.EqualTo(Path.Combine(file.Directory.FullName, ".net-app")));
        } 
        
        
        [Test]
        public void CanAddBundlePathConfig()
        {
            File.WriteAllText("sample.xml", @"<?xml version=""1.0"" encoding=""utf-8""?>
<!--
  For more information on how to configure your ASP.NET application, please visit
  http://go.microsoft.com/fwlink/?LinkId=169433
  -->
<configuration>
  <system.web>
    <httpRuntime maxRequestLength=""307200"" maxQueryStringLength=""10240"" />
  </system.web>
  <system.webServer>
    <security>
      <requestFiltering>
        <requestLimits maxAllowedContentLength=""314572800"" maxUrl=""10241"" maxQueryString=""20481"" />
      </requestFiltering>
    </security>
    <handlers>
      <add name=""aspNetCore"" path=""*"" verb=""*"" modules=""AspNetCoreModuleV2"" resourceType=""Unspecified"" />
    </handlers>
    <aspNetCore processPath=""%LAUNCHER_PATH%"" stdoutLogEnabled=""false"" stdoutLogFile="".\logs\stdout"" arguments=""%LAUNCHER_ARGS%"">
    </aspNetCore>
  </system.webServer>
</configuration>");

            var file = new FileInfo("sample.xml");

            CustomActions.UpdateWebConfigContent(file.FullName);

            var xml = File.ReadAllText(file.FullName);
            var xdoc = XDocument.Parse(xml).Descendants("environmentVariable")
                .Single(x => x.Attribute("name")?.Value == "DOTNET_BUNDLE_EXTRACT_BASE_DIR");

            Assert.That(xdoc.Attribute("value").Value, Is.EqualTo(Path.Combine(file.Directory.FullName, ".net-app")));
        }
    }
}
