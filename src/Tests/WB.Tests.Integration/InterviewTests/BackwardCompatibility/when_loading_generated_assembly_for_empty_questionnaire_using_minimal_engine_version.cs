using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;

namespace WB.Tests.Integration.InterviewTests.BackwardCompatibility
{
    internal class when_loading_generated_assembly_for_empty_questionnaire_using_minimal_engine_version : in_standalone_app_domain
    {
        Because of = () => results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
        {
            Setup.MockedServiceLocator();
            QuestionnaireDocument questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter();
            Assembly assembly = CompileAssemblyUsingQuestionnaireEngine(questionnaireDocument);

            var questionnaireTopLevelTypeInfo = assembly.GetTypes().Single(type => type.Name == "QuestionnaireTopLevel").GetTypeInfo();

            return new InvokeResults { QuestionnaireTopLevelContainsGetRosterKeyMethod = questionnaireTopLevelTypeInfo.DeclaredMethods.Any(method => method.Name.EndsWith("GetRosterKey")) };
        });

        It should_declare_GetRosterKey_method_in_QuestionnaireTopLevel = () =>
            results.QuestionnaireTopLevelContainsGetRosterKeyMethod.ShouldBeTrue();

        private static InvokeResults results;

        [Serializable]
        internal class InvokeResults
        {
            public bool QuestionnaireTopLevelContainsGetRosterKeyMethod { get; set; }
        }
    }
}
