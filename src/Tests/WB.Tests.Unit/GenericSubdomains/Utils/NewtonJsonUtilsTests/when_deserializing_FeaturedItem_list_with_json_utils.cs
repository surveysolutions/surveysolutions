﻿using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.BoundedContexts.Interviewer.ViewModel.Dashboard;

namespace WB.Tests.Unit.GenericSubdomains.Utils.NewtonJsonUtilsTests
{
    internal class when_deserializing_FeaturedItem_list_with_json_utils : NewtonJsonUtilsTestContext
    {
        Establish context = () =>
        {
            jsonUtils =
                CreateNewtonJsonUtils(new Dictionary<string, string>()
                {
                    {
                        "WB.UI.Capi",
                        "WB.Core.BoundedContexts.Interviewer"
                    }
                });
        };

        Because of = () =>
            result = jsonUtils.Deserialize<IEnumerable<FeaturedItem>>(serializedQuestionnaire);

        It should_return_not_null_result = () =>
            result.ShouldNotBeNull();

        private const string serializedQuestionnaire = @"[{""$type"":""WB.UI.Capi.ViewModel.Dashboard.FeaturedItem, WB.UI.Capi"",""PublicKey"":""41a029ff-d7a0-724d-6668-902256c82197"",""Title"":""Prefilled"",""Value"":""first""}]";
        private static NewtonJsonUtils jsonUtils;
        private static IEnumerable<FeaturedItem> result;
    }
}