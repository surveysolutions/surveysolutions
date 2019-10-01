using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;
using WB.UI.Headquarters.API.PublicApi.Models;
using System.Collections.Generic;
using NSubstitute;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Assignment;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Tests.Web;

namespace WB.Tests.Unit.Applications.Headquarters.PublicApiTests.AssignmentsTests
{
    public class when_create_assignment_with_AssignmentsPublicApi : BaseAssignmentsControllerTest
    {
        [Test]
        public void should_return_404_for_non_existing_responsibleUser()
        {
            Assert.Throws(Is.TypeOf<HttpResponseException>()
                    .And.Property(nameof(HttpResponseException.Response))
                    .Property(nameof(HttpResponseMessage.StatusCode)).EqualTo(HttpStatusCode.NotFound),
                () => this.controller.Create(new CreateAssignmentApiRequest()
                {
                    Responsible = "any"
                }));
        }

        [TestCase(UserRoles.Administrator)]
        [TestCase(UserRoles.Headquarter)]
        [TestCase(UserRoles.Observer)]
        [TestCase(UserRoles.ApiUser)]
        public void should_return_406_for_non_interviewer_or_supervisor_assignee(UserRoles role)
        {
            this.SetupResponsibleUser(Create.Entity.HqUser(role: role));

            Assert.Throws(Is.TypeOf<HttpResponseException>()
                    .And.Property(nameof(HttpResponseException.Response))
                    .Property(nameof(HttpResponseMessage.StatusCode)).EqualTo(HttpStatusCode.NotAcceptable),
                () => this.controller.Create(new CreateAssignmentApiRequest
                {
                    Responsible = "any"
                }));
        }

        [TestCase("bad_crafted_questionnarie_id")]
        [TestCase("f2250674-42e6-4756-b394-b86caa62225e$1")]
        public void should_return_404_for_non_existing_questionnaire(string questionnaireId)
        {
            this.SetupResponsibleUser(Create.Entity.HqUser());

            Assert.Throws(Is.TypeOf<HttpResponseException>()
                    .And.Property(nameof(HttpResponseException.Response))
                    .Property(nameof(HttpResponseMessage.StatusCode)).EqualTo(HttpStatusCode.NotFound),
                () => this.controller.Create(new CreateAssignmentApiRequest
                {
                    QuestionnaireId = questionnaireId,
                    Responsible = "any"
                }));
        }

        [Test]
        public async Task should_return_failed_verification_results_with_400_code()
        {
            var qid = QuestionnaireIdentity.Parse("f2250674-42e6-4756-b394-b86caa62225e$1");

            this.SetupResponsibleUser(Create.Entity.HqUser());
            this.SetupQuestionnaire(Create.Entity.QuestionnaireDocument());

            var assignment = Create.Entity.Assignment(1, qid);

            this.mapper
                .Setup(m => m.Map(It.IsAny<CreateAssignmentApiRequest>(), It.IsAny<Assignment>()))
                .Returns(assignment);

            this.interviewImportService
                .Setup(x => x.VerifyWithInterviewTree(It.IsAny<List<InterviewAnswer>>(), It.IsAny<Guid?>(), It.IsAny<IQuestionnaire>()))
                .Returns(new InterviewImportError("PL0011", "error"));

            try
            {
                this.controller.Create(new CreateAssignmentApiRequest
                {
                    QuestionnaireId = qid.ToString(),
                    Responsible = "any"
                });
            }
            catch (HttpResponseException hre)
            {
                Assert.That(hre.Response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

                var response = await hre.Response.Content.ReadAsAsync<CreateAssignmentResult>();
                Assert.That(response.VerificationStatus.Errors, Has.Count.EqualTo(1));
            }
        }

        [Test]
        public void should_store_new_assignment()
        {
            var qid = QuestionnaireIdentity.Parse("f2250674-42e6-4756-b394-b86caa62225e$1");

            this.SetupResponsibleUser(Create.Entity.HqUser());
            this.SetupQuestionnaire(Create.Entity.QuestionnaireDocument());

            var assignment = Create.Entity.Assignment(1, qid);

            this.mapper
                .Setup(m => m.Map(It.IsAny<CreateAssignmentApiRequest>(), It.IsAny<Assignment>()))
                .Returns(assignment);

            this.interviewImportService
                .Setup(x => x.VerifyWithInterviewTree(It.IsAny<List<InterviewAnswer>>(), It.IsAny<Guid?>(), It.IsAny<IQuestionnaire>()))
                .Returns((InterviewImportError)null);

            this.controller.Create(new CreateAssignmentApiRequest
            {
                QuestionnaireId = qid.ToString(),
                Responsible = "any"
            });

            this.commandService.Verify(ass => ass.Execute(It.IsAny<CreateAssignment>(), null), Times.Once);
        }

        [Test]
        public void and_quantity_minus_1_should_stored_new_assignment_has_null_quantity()
        {
            var qid = QuestionnaireIdentity.Parse("f2250674-42e6-4756-b394-b86caa62225e$1");

            this.SetupResponsibleUser(Create.Entity.HqUser());
            this.SetupQuestionnaire(Create.Entity.QuestionnaireDocument());

            var assignment = Create.Entity.Assignment(1, qid);

            this.mapper
                .Setup(m => m.Map(It.IsAny<CreateAssignmentApiRequest>(), It.IsAny<Assignment>()))
                .Returns(assignment);

            this.interviewImportService
                .Setup(x => x.VerifyWithInterviewTree(It.IsAny<List<InterviewAnswer>>(), It.IsAny<Guid?>(), It.IsAny<IQuestionnaire>()))
                .Returns((InterviewImportError)null);

            this.controller.Create(new CreateAssignmentApiRequest
            {
                QuestionnaireId = qid.ToString(),
                Responsible = "any",
                Quantity = -1
            });

            this.commandService.Verify(ass => ass.Execute(It.Is<CreateAssignment>(x=>x.Quantity == null), null), Times.Once);
        }
    }
}
