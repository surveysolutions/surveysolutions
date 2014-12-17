using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.Android;

namespace WB.UI.QuestionnaireTester.Tests
{
    [Ignore]
    [TestFixture]
    public class SmokeTest
    {
        private static string apiKey = "226353171d1db2a71de75bb6641a6138";

        [Test]
        public void Run_SmokeTest_Successfully()
        {
            var app = ConfigureApp
                .Android
                .ApkFile(ConfigurationManager.AppSettings["apkPath"])
                .ApiKey(apiKey)
                .StartApp();

            app.EnterText(c => c.Id("teLogin"), "nastya_k");
            app.EnterText(c => c.Id("tePassword"), "Nastya1234");
            app.Tap(c => c.Id("btnLogin"));
            app.WaitForElement(c => c.Text("Business_test"));
            app.Tap(c => c.Text("Business_test"));
            app.WaitForElement(c => c.Text("New Question"));
            app.Tap(c => c.TextField());
            app.EnterText("n");
            app.EnterText("a");
            app.EnterText("s");
            app.EnterText("t");
            app.EnterText("y");
            app.EnterText("a");
            app.PressUserAction(UserAction.Done);
            app.SwipeRight();
            
            var result = app.Query(c => c.Text("1/5"));
            Assert.That(result.Count(), Is.EqualTo(1));
        }
    }
}
