using System;
using AppDomainToolkit;
using Main.Core.Entities.Composite;
using NUnit.Framework;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.Rosters
{
    public class KP_10925_when_compile_roster_with_Level_Prefix : InterviewTestsContext
    {
        [OneTimeSetUp]
        public void Context()
        {
            appDomainContext = AppDomainContext.Create();
        }

        [OneTimeTearDown]
        public void ClenupStuff()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        [Test]
        public void should_compile_without_WB0096_error()
        {
            Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(new IComposite[]
                {
                    Create.Entity.Roster(rosterGroupId, "PLOTS", variable: "PLOTS",
                        children: new IComposite[]
                        {
                            Create.Entity.NumericIntegerQuestion(nestedQuestion)
                        }),

                    Create.Entity.NumericIntegerQuestion(rosterSource, variable: "Level_PLOTS")
                });

                Assert.DoesNotThrow(() => SetupInterview(questionnaireDocument));

                return true;
            });
        }

        private static readonly Guid rosterGroupId = Id.gA;
        private static readonly Guid rosterSource = Id.g1;
        private static readonly Guid nestedQuestion = Id.g2;
        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
    }
}
