using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.V2;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewExpressionStateTests
{
    internal class when_getting_methods_of_IInterviewExpressionState
    {
        [NUnit.Framework.OneTimeSetUp]
        public void context()
        {
            BecauseOf();
        }

        public void BecauseOf() =>
            methods = typeof(IInterviewExpressionStateV2).GetMethods()
                .Concat(typeof(IInterviewExpressionStateV2)
                    .GetInterfaces()
                    .SelectMany(i => i.GetMethods()))
                .Select(x => new MetodInfo
                {
                    Name = x.Name,
                    ReturnType = x.ReturnType,
                    ParamsType = x.GetParameters().Select(p => p.ParameterType).ToArray()
                })
                .ToList();

        [NUnit.Framework.Test] public void should_return_list_of_specified_methods_only () =>
            methods.Select(m => m.Name).Should().BeEquivalentTo(interfaceMethods.Select(x => x.Name));
        [NUnit.Framework.Test] public void should_match_method_signature_for_UpdateNumericRealAnswer () =>
            methods.Get("UpdateNumericRealAnswer").ShouldMatchMethodInfo(interfaceMethods.Get("UpdateNumericRealAnswer"));
        [NUnit.Framework.Test] public void should_match_method_signature_for_UpdateDateAnswer () =>
            methods.Get("UpdateDateAnswer").ShouldMatchMethodInfo(interfaceMethods.Get("UpdateDateAnswer"));
        [NUnit.Framework.Test] public void should_match_method_signature_for_UpdateMediaAnswer () =>
            methods.Get("UpdateMediaAnswer").ShouldMatchMethodInfo(interfaceMethods.Get("UpdateMediaAnswer"));
        [NUnit.Framework.Test] public void should_match_method_signature_for_UpdateTextAnswer () =>
            methods.Get("UpdateTextAnswer").ShouldMatchMethodInfo(interfaceMethods.Get("UpdateTextAnswer"));
        [NUnit.Framework.Test] public void should_match_method_signature_for_UpdateQrBarcodeAnswer () =>
            methods.Get("UpdateQrBarcodeAnswer").ShouldMatchMethodInfo(interfaceMethods.Get("UpdateQrBarcodeAnswer"));
        [NUnit.Framework.Test] public void should_match_method_signature_for_UpdateSingleOptionAnswer () =>
            methods.Get("UpdateSingleOptionAnswer").ShouldMatchMethodInfo(interfaceMethods.Get("UpdateSingleOptionAnswer"));
        [NUnit.Framework.Test] public void should_match_method_signature_for_UpdateMultiOptionAnswer () =>
            methods.Get("UpdateMultiOptionAnswer").ShouldMatchMethodInfo(interfaceMethods.Get("UpdateMultiOptionAnswer"));
        [NUnit.Framework.Test] public void should_match_method_signature_for_UpdateGeoLocationAnswer () =>
            methods.Get("UpdateGeoLocationAnswer").ShouldMatchMethodInfo(interfaceMethods.Get("UpdateGeoLocationAnswer"));
        [NUnit.Framework.Test] public void should_match_method_signature_for_UpdateTextListAnswer () =>
            methods.Get("UpdateTextListAnswer").ShouldMatchMethodInfo(interfaceMethods.Get("UpdateTextListAnswer"));
        [NUnit.Framework.Test] public void should_match_method_signature_for_UpdateLinkedSingleOptionAnswer () =>
            methods.Get("UpdateLinkedSingleOptionAnswer").ShouldMatchMethodInfo(interfaceMethods.Get("UpdateLinkedSingleOptionAnswer"));
        [NUnit.Framework.Test] public void should_match_method_signature_for_UpdateLinkedMultiOptionAnswer () =>
            methods.Get("UpdateLinkedMultiOptionAnswer").ShouldMatchMethodInfo(interfaceMethods.Get("UpdateLinkedMultiOptionAnswer"));

        [NUnit.Framework.Test] public void should_match_method_signature_for_DeclareAnswersInvalid () =>
            methods.Get("DeclareAnswersInvalid").ShouldMatchMethodInfo(interfaceMethods.Get("DeclareAnswersInvalid"));
        [NUnit.Framework.Test] public void should_match_method_signature_for_DeclareAnswersValid () =>
            methods.Get("DeclareAnswersValid").ShouldMatchMethodInfo(interfaceMethods.Get("DeclareAnswersValid"));

        [NUnit.Framework.Test] public void should_match_method_signature_for_DisableGroups () =>
            methods.Get("DisableGroups").ShouldMatchMethodInfo(interfaceMethods.Get("DisableGroups"));
        [NUnit.Framework.Test] public void should_match_method_signature_for_EnableGroups () =>
            methods.Get("EnableGroups").ShouldMatchMethodInfo(interfaceMethods.Get("EnableGroups"));
        [NUnit.Framework.Test] public void should_match_method_signature_for_DisableQuestions () =>
            methods.Get("DisableQuestions").ShouldMatchMethodInfo(interfaceMethods.Get("DisableQuestions"));
        [NUnit.Framework.Test] public void should_match_method_signature_for_EnableQuestions () =>
            methods.Get("EnableQuestions").ShouldMatchMethodInfo(interfaceMethods.Get("EnableQuestions"));


        [NUnit.Framework.Test] public void should_match_method_signature_for_AddRoster () =>
            methods.Get("AddRoster").ShouldMatchMethodInfo(interfaceMethods.Get("AddRoster"));
        [NUnit.Framework.Test] public void should_match_method_signature_for_RemoveRoster () =>
            methods.Get("RemoveRoster").ShouldMatchMethodInfo(interfaceMethods.Get("RemoveRoster"));
        
        [NUnit.Framework.Test] public void should_match_method_signature_for_ProcessValidationExpressions () =>
            methods.Get("ProcessValidationExpressions").ShouldMatchMethodInfo(interfaceMethods.Get("ProcessValidationExpressions"));
        [NUnit.Framework.Test] public void should_match_method_signature_for_ProcessEnablementConditions () =>
            methods.Get("ProcessEnablementConditions").ShouldMatchMethodInfo(interfaceMethods.Get("ProcessEnablementConditions"));
        [NUnit.Framework.Test] public void should_match_method_signature_for_SaveAllCurrentStatesAsPrevious () =>
            methods.Get("SaveAllCurrentStatesAsPrevious").ShouldMatchMethodInfo(interfaceMethods.Get("SaveAllCurrentStatesAsPrevious"));

        [NUnit.Framework.Test] public void should_match_method_signature_for_Clone_of_first_version () =>
            methods.GetByMethodNameAndReturnType("Clone", typeof(IInterviewExpressionState)).ShouldMatchMethodInfo(interfaceMethods.GetByMethodNameAndReturnType("Clone", typeof(IInterviewExpressionState)));
        [NUnit.Framework.Test] public void should_match_method_signature_for_Clone_of_second_version () =>
           methods.GetByMethodNameAndReturnType("Clone", typeof(IInterviewExpressionStateV2)).ShouldMatchMethodInfo(interfaceMethods.GetByMethodNameAndReturnType("Clone", typeof(IInterviewExpressionStateV2)));

        private static List<MetodInfo> methods;

        private static List<MetodInfo> interfaceMethods = new List<MetodInfo>
        {
            new MetodInfo
            {
                Name = "UpdateNumericIntegerAnswer",
                ReturnType = typeof (void),
                ParamsType = new Type[]{ typeof (Guid), typeof (decimal[]), typeof (long?)}
            },
            new MetodInfo
            {
                Name = "UpdateNumericRealAnswer",
                ReturnType = typeof (void),
                ParamsType = new Type[] { typeof (Guid), typeof (decimal[]), typeof (double?) }
            },
            new MetodInfo
            {
                Name = "UpdateDateAnswer",
                ReturnType = typeof (void),
                ParamsType = new Type[] { typeof (Guid), typeof (decimal[]), typeof (DateTime?) }
            },
            new MetodInfo
            {
                Name = "UpdateMediaAnswer",
                ReturnType = typeof (void),
                ParamsType = new Type[] { typeof (Guid), typeof (decimal[]), typeof (string) }
            },
            new MetodInfo
            {
                Name = "UpdateTextAnswer",
                ReturnType = typeof (void),
                ParamsType = new Type[] { typeof (Guid), typeof (decimal[]), typeof (string) }
            },
            new MetodInfo
            {
                Name = "UpdateQrBarcodeAnswer",
                ReturnType = typeof (void),
                ParamsType = new Type[] { typeof (Guid), typeof (decimal[]), typeof (string) }
            },
            new MetodInfo
            {
                Name = "UpdateSingleOptionAnswer",
                ReturnType = typeof (void),
                ParamsType = new Type[] { typeof (Guid), typeof (decimal[]), typeof (decimal?) }
            },
            new MetodInfo
            {
                Name = "UpdateMultiOptionAnswer",
                ReturnType = typeof (void),
                ParamsType = new Type[] { typeof (Guid), typeof (decimal[]), typeof (decimal[]) }
            },
            new MetodInfo
            {
                Name = "UpdateGeoLocationAnswer",
                ReturnType = typeof (void),
                ParamsType = new Type[] { typeof (Guid), typeof (decimal[]), typeof (double), typeof (double), typeof (double), typeof (double) }
            },
            new MetodInfo
            {
                Name = "UpdateTextListAnswer",
                ReturnType = typeof (void),
                ParamsType = new Type[] { typeof (Guid), typeof (decimal[]), typeof (Tuple<decimal, string>[]) }
            },
            new MetodInfo
            {
                Name = "UpdateLinkedSingleOptionAnswer",
                ReturnType = typeof (void),
                ParamsType = new Type[] { typeof (Guid), typeof (decimal[]), typeof (decimal[]) }
            },
            new MetodInfo
            {
                Name = "UpdateLinkedMultiOptionAnswer",
                ReturnType = typeof (void),
                ParamsType = new Type[] { typeof (Guid), typeof (decimal[]), typeof (decimal[][]) }
            },


            new MetodInfo
            {
                Name = "DeclareAnswersInvalid",
                ReturnType = typeof (void),
                ParamsType = new Type[] { typeof (IEnumerable<Identity>) }
            },
            new MetodInfo
            {
                Name = "DeclareAnswersValid",
                ReturnType = typeof (void),
                ParamsType = new Type[] { typeof (IEnumerable<Identity>)}
            },
            new MetodInfo
            {
                Name = "DisableGroups",
                ReturnType = typeof (void),
                ParamsType = new Type[] {typeof (IEnumerable<Identity>) }
            },
            new MetodInfo
            {
                Name = "EnableGroups",
                ReturnType = typeof (void),
                ParamsType = new Type[] {typeof (IEnumerable<Identity>) }
            },
            new MetodInfo
            {
                Name = "DisableQuestions",
                ReturnType = typeof (void),
                ParamsType = new Type[] { typeof (IEnumerable<Identity>)}
            },
            new MetodInfo
            {
                Name = "EnableQuestions",
                ReturnType = typeof (void),
                ParamsType = new Type[] { typeof (IEnumerable<Identity>) }
            },
            new MetodInfo
            {
                Name = "AddRoster",
                ReturnType = typeof (void),
                ParamsType = new Type[] { typeof (Guid), typeof (decimal[]), typeof (decimal), typeof(int?) }
            },
            new MetodInfo
            {
                Name = "RemoveRoster",
                ReturnType = typeof (void),
                ParamsType = new Type[] { typeof (Guid), typeof (decimal[]), typeof (decimal) }
            },
            new MetodInfo
            {
                Name = "ProcessValidationExpressions",
                ReturnType = typeof (ValidityChanges),
                ParamsType = new Type[0] 
            },
            new MetodInfo
            {
                Name = "ProcessEnablementConditions",
                ReturnType = typeof (EnablementChanges),
                ParamsType =  new Type[0] 
            },
            new MetodInfo
            {
                Name = "SaveAllCurrentStatesAsPrevious",
                ReturnType = typeof (void),
                ParamsType =  new Type[0] 
            },
            new MetodInfo { Name = "Clone", ReturnType = typeof (IInterviewExpressionState), ParamsType = new Type[0] },
            new MetodInfo { Name = "Clone", ReturnType = typeof (IInterviewExpressionStateV2), ParamsType = new Type[0] },
            new MetodInfo
            {
                Name = "UpdateRosterTitle",
                ReturnType = typeof (void),
                ParamsType =  new Type[] { typeof (Guid), typeof (decimal[]), typeof (string) } 
            }
        };
    }
}
