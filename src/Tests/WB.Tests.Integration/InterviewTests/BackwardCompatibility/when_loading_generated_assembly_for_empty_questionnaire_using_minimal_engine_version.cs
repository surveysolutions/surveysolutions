using System;
using System.Linq;
using System.Reflection;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Documents;
using NUnit.Framework;

namespace WB.Tests.Integration.InterviewTests.BackwardCompatibility
{
    internal class when_loading_generated_assembly_for_empty_questionnaire_using_minimal_engine_version : in_standalone_app_domain
    {
        [Test]
        public void should_declare_GetRosterKey_method_in_QuestionnaireTopLevel()
        {
            appDomainContext = AppDomainContext.Create();

            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();
                QuestionnaireDocument questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter();
                Assembly assembly = CompileAssemblyUsingQuestionnaireEngine(questionnaireDocument);

                var questionnaireTopLevelTypeInfo =
                    assembly.GetTypes().Single(type => type.Name == "QuestionnaireTopLevel").GetTypeInfo();

                return new InvokeResults
                {
                    QuestionnaireTopLevelContainsGetRosterKeyMethod =
                        questionnaireTopLevelTypeInfo.DeclaredMethods.Any(method => method.Name.EndsWith("GetRosterKey"))
                };
            });

            results.QuestionnaireTopLevelContainsGetRosterKeyMethod.Should().BeTrue();
        }

        private static InvokeResults results;

        [Serializable]
        internal class InvokeResults
        {
            public bool QuestionnaireTopLevelContainsGetRosterKeyMethod { get; set; }
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }
    }
}
