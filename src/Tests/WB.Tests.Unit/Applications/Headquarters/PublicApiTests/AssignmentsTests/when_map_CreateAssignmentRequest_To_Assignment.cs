using System;
using System.Collections.Generic;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Tests.Abc;
using WB.UI.Headquarters.API.PublicApi.Models;

namespace WB.Tests.Unit.Applications.Headquarters.PublicApiTests.AssignmentsTests
{
    public class when_map_CreateAssignmentRequest_To_Assignment : AssignmentsPublicApiMapProfileSpecification
    {
        private CreateAssignmentApiRequest CreateRequest;
        private Assignment Assignment;

        public override void Context()
        {
            this.CreateRequest = new CreateAssignmentApiRequest
            {
                IdentifyingData = new List<AssignmentIdentifyingDataItem>
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
                }
            };

            this.Assignment = Create.Entity.Assignment(1, Create.Entity.QuestionnaireIdentity(Id.gA, 1));
        }
        
        public override void Because()
        {
            this.Assignment = this.mapper.Map(this.CreateRequest, this.Assignment);
        }

        [Test]
        public void should_map_answers()
        {
            Assert.That(this.Assignment.IdentifyingData.Count, Is.EqualTo(2));
            Assert.That(this.Assignment.IdentifyingData[0].Answer, Is.EqualTo("Test1"));
            Assert.That(this.Assignment.IdentifyingData[1].Answer, Is.EqualTo("Test1"));
        }

        [Test]
        public void should_map_questionIds_according_to_questionnaire()
        {
            Assert.That(this.Assignment.IdentifyingData[0].QuestionId, Is.EqualTo(Id.g2));
            Assert.That(this.Assignment.IdentifyingData[1].QuestionId, Is.EqualTo(Id.g3));
        }


        [Test]
        public void should_map_variableNames_according_to_questionnaire()
        {
            Assert.That(this.Assignment.IdentifyingData[0].VariableName, Is.EqualTo("test2"));
            Assert.That(this.Assignment.IdentifyingData[1].VariableName, Is.EqualTo("test3"));
        }
    }
}