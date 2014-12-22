using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.Utils;

namespace WB.Tests.Unit.SharedKernels.DataCollection.RepositoryKeyHelperTests
{
    public class when_creating_key_for_variable_by_questionnaire_readside_versioned_repositories
    {
        Establish context = () => { };

        Because of = () =>
            key = RepositoryKeysHelper.GetVariableByQuestionnaireKey("var", "11111111111111111111111111111111-1");

        It should_create_key_var_11111111111111111111111111111111_1 = () =>
            key.ShouldEqual("var-11111111111111111111111111111111-1");

        private static string key;
    }
}