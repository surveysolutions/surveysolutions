using System.Collections.Generic;
using AutoMapper;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Tests.Abc;
using WB.UI.Headquarters.API.PublicApi.Models;

namespace WB.Tests.Unit.Applications.Headquarters.PublicApiTests.AssignmentsTests
{
    public class when_map_CreateAssignmentApiRequest_WithNonExistingQuestions : AssignmentsPublicApiMapProfileSpecification
    {
        private CreateAssignmentApiRequest CreateAssignment;
        private Assignment Assignment;

        public override void Context()
        {
            this.CreateAssignment = new CreateAssignmentApiRequest
            {
                IdentifyingData = new List<AssignmentIdentifyingDataItem>
                {
                    new AssignmentIdentifyingDataItem
                    {
                        Answer = "Test1",
                        Variable = "nonExsiting"
                    }
                }
            };

            this.Assignment = Create.Entity.Assignment(1, Create.Entity.QuestionnaireIdentity(Id.gA, 1));
        }

        public override void Because() { }

        [Test]
        public void Should_Throw_Exception()
        {
            try
            {
                this.Assignment = this.mapper.Map(this.CreateAssignment, this.Assignment);
            }
            catch (AutoMapperMappingException ame)
            {
                Assert.That(ame.InnerException.Message, Does.Contain("Cannot identify question from provided data"));
                return;
            }
            Assert.Fail();
        }
    }
}