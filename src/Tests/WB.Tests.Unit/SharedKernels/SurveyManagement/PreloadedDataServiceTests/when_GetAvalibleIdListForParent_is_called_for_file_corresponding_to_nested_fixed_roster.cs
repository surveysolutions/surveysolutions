using System;
using System.Linq;
using Main.Core.Entities.Composite;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataServiceTests
{
    internal class when_GetAvalibleIdListForParent_is_called_for_file_corresponding_to_nested_fixed_roster : PreloadedDataServiceTestContext
    {
        [Test]
        public void Should_not_return_null_result()
        {
            var rosterGroupId = Id.g1;
            var nestedRosterId = Id.g2;
            var questionnaireDocument =
                CreateQuestionnaireDocumentWithOneChapter(

                    Create.Entity.FixedRoster(rosterId: rosterGroupId, title: "rosterGroup", variable: "rosterGroup",
                        fixedTitles: new[] {Create.Entity.FixedTitle(1), Create.Entity.FixedTitle(2)},
                        children: new IComposite[]
                        {
                            Create.Entity.FixedRoster(rosterId: nestedRosterId, variable: "nestedRoster",
                                fixedTitles: new[] {Create.Entity.FixedTitle(3), Create.Entity.FixedTitle(4)},
                                title: "nestedRoster")
                        }));


            var importDataParsingService = CreatePreloadedDataService(questionnaireDocument);
            
            var result = importDataParsingService.GetAvailableIdListForParent(
                CreatePreloadedDataByFile(
                    header: new string[] {"nestedroster__id", "rosterGroup__id", ServiceColumns.InterviewId},
                    content: new string[][]
                    {
                        new string[] {"1", "1", "1"}
                    },
                    fileName: "nestedRoster"),
                new ValueVector<Guid> {rosterGroupId, nestedRosterId}, new[] {"1", "1", "1"},
                Create.Entity.PreloadedDataByFile(new PreloadedDataByFile[0]));


            Assert.IsNotNull(result);
            Assert.IsTrue(result.SequenceEqual(new[] { 3, 4 }));
            
        }
    }
}
