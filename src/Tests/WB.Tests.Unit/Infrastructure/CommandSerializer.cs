using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Tests.Abc;
using WB.UI.WebTester.Infrastructure.AppDomainSpecific;

namespace WB.Tests.Unit.Infrastructure
{
    public class CommandSerializer
    {
        [Test]
        public void test_name()
        {
            var command = new CreateInterview(
                interviewId: Id.g1,
                userId: Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"),
                questionnaireId: Create.Entity.QuestionnaireIdentity(),
                answers: new List<InterviewAnswer>(),
                
                protectedVariables: new List<string>(), 
                supervisorId: Guid.NewGuid(),
                interviewerId: Guid.NewGuid(),
                interviewKey: Create.Entity.InterviewKey(),
                assignmentId: null);

            string serializeObject = JsonConvert.SerializeObject(command, Formatting.Indented, new JsonSerializerSettings());

            var desCommand = JsonConvert.DeserializeObject<CreateInterview>(serializeObject, new JsonSerializerSettings
            {
                ContractResolver = new PrivateSetterContractResolver()
            });

            Assert.That(desCommand.InterviewId, Is.EqualTo(Id.g1));
            Assert.That(serializeObject, Does.Contain(Id.gA.ToString()));
        }
    }
}
