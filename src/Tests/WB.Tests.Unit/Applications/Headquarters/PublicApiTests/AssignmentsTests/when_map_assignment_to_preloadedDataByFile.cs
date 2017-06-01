using System.Collections.Generic;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.PreloadedData;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Applications.Headquarters.PublicApiTests.AssignmentsTests
{
    public class when_map_assignment_to_preloadedDataByFile : AssignmentsPublicApiMapProfileSpecification
    {
        protected Assignment Assignment { get; set; }
        protected PreloadedDataByFile PreloadedDataByFile { get; set; }

        public override void Context()
        {
            this.Assignment = Create.Entity.Assignment(1, Create.Entity.QuestionnaireIdentity(Id.g1, 10),
                responsibleName: "TestName",
                interviewSummary: new HashSet<InterviewSummary>{
                    new InterviewSummary(),
                    new InterviewSummary()
                });

            this.Assignment.SetAnswers(new List<IdentifyingAnswer>
            {
                Create.Entity.IdentifyingAnswer(this.Assignment, answer: "Test22", questionId: Id.g2),
                Create.Entity.IdentifyingAnswer(this.Assignment, answer: "Test33", questionId: Id.g3)
            });
        }

        public override void Because()
        {
            this.PreloadedDataByFile = this.mapper.Map<PreloadedDataByFile>(this.Assignment);
        }

        [Test]
        public void should_set_headers_with_variable_names() => Assert.That(this.PreloadedDataByFile.Header, Is.EqualTo(new []{"test2", "test3"}));

        [Test]
        public void should_set_content_from_answers() => Assert.That(this.PreloadedDataByFile.Content[0], Is.EqualTo(new[] { "Test22", "Test33" }));
    }
}