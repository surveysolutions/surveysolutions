using System.Collections.Generic;
using Machine.Specifications;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Tests.Abc;
using WB.UI.Headquarters.API.PublicApi.Models;

namespace WB.Tests.Unit.Applications.Headquarters.PublicApiTests.AssignmentsTests
{
    public class when_map_AssignmentIdentifyingDataItem_List_To_Assignment : AssignmentsPublicApiMapProfileSpecification
    {
        private List<AssignmentIdentifyingDataItem> DataItems;
        private Assignment Assignment;

        public override void Context()
        {
            this.DataItems = new List<AssignmentIdentifyingDataItem>
            {
                new AssignmentIdentifyingDataItem
                {
                    Answer = "Test1",
                    Variable = "test2"
                },
                new AssignmentIdentifyingDataItem
                {
                    Answer = "Test1",
                    QuestionId = Id.g3
                }
            };

            this.Assignment = Create.Entity.Assignment(1, Create.Entity.QuestionnaireIdentity(Id.gA, 1));
        }

        public override void Because()
        {
            this.Assignment = this.mapper.Map(this.DataItems, this.Assignment);
        }

        [Test]
        public void should_map_answers()
        {
            this.Assignment.IdentifyingData.Count.ShouldEqual(2);
            this.Assignment.IdentifyingData[0].Answer.ShouldEqual("Test1");
            this.Assignment.IdentifyingData[1].Answer.ShouldEqual("Test1");
        }

        [Test]
        public void should_map_questionIds_according_to_questionnaire()
        {
            this.Assignment.IdentifyingData[0].QuestionId.ShouldEqual(Id.g2);
            this.Assignment.IdentifyingData[1].QuestionId.ShouldEqual(Id.g3);
        }


        [Test]
        public void should_map_variableNames_according_to_questionnaire()
        {
            this.Assignment.IdentifyingData[0].VariableName.ShouldEqual("test2");
            this.Assignment.IdentifyingData[1].VariableName.ShouldEqual("test3");
        }
    }
}