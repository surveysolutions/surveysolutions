using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Core.SharedKernels.DataCollection.Commands.Assignment;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;
using WB.Tests.Unit.Applications.Headquarters.PublicApiTests.AssignmentsTests;
using WB.UI.Headquarters.API.PublicApi.Models;

namespace WB.Tests.Web.Headquarters.Controllers.PublicApiTests.AssignmentsTests
{
    public class when_create_assignment_with_AssignmentsPublicApi : BaseAssignmentsControllerTest
    {
        [Test]
        public void should_return_400_for_non_existing_responsibleUser()
        {
            var qid = QuestionnaireIdentity.Parse("f2250674-42e6-4756-b394-b86caa62225e$1");
            
            this.SetupQuestionnaire(Abc.Create.Entity.QuestionnaireDocument());
            var result = this.controller.Create(new CreateAssignmentApiRequest()
            {
                QuestionnaireId = qid.ToString(),
                Responsible = "any"
            });
            
            Assert.That(result.Result, Has.Property(nameof(IStatusCodeActionResult.StatusCode)).EqualTo(StatusCodes.Status400BadRequest));
            var verificationErrors = (((ObjectResult) result.Result).Value as CreateAssignmentResult)
                ?.VerificationStatus.Errors;
            
            Assert.That(verificationErrors, Is.Not.Empty);
            Assert.That(verificationErrors?[0].Code, Is.EqualTo("PL0026"));
        }

        [TestCase(UserRoles.Administrator)]
        [TestCase(UserRoles.Observer)]
        [TestCase(UserRoles.ApiUser)]
        public void should_return_400_for_non_interviewer_or_supervisor_or_headquarters_assignee(UserRoles role)
        {
            var qid = QuestionnaireIdentity.Parse("f2250674-42e6-4756-b394-b86caa62225e$1");
            
            this.SetupQuestionnaire(Abc.Create.Entity.QuestionnaireDocument());
            var hqUser = Abc.Create.Entity.HqUser(role: role);
            this.SetupResponsibleUser(hqUser);

            var result = this.controller.Create(new CreateAssignmentApiRequest
            {
                QuestionnaireId = qid.ToString(),
                Responsible = hqUser.UserName
            });
            
            Assert.That(result.Result, Has.Property(nameof(IStatusCodeActionResult.StatusCode)).EqualTo(StatusCodes.Status400BadRequest));
            var verificationErrors = (((ObjectResult) result.Result).Value as CreateAssignmentResult)
                ?.VerificationStatus.Errors;
            
            Assert.That(verificationErrors, Is.Not.Empty);
            Assert.That(verificationErrors?[0].Code, Is.EqualTo("PL0028"));
        }

        [TestCase("bad_crafted_questionnaire_id")]
        [TestCase("f2250674-42e6-4756-b394-b86caa62225e$1")]
        public void should_return_404_for_non_existing_questionnaire(string questionnaireId)
        {
            this.SetupResponsibleUser(Abc.Create.Entity.HqUser());
            var result = this.controller.Create(new CreateAssignmentApiRequest
            {
                QuestionnaireId = questionnaireId,
                Responsible = "any"
            });
            
            Assert.That(result.Result, Has.Property(nameof(IStatusCodeActionResult.StatusCode)).EqualTo(StatusCodes.Status404NotFound));
        }

        [Test]
        public void when_invalid_in_interview_tree_then_should_return_failed_verification_results_with_400_code()
        {
            var qid = QuestionnaireIdentity.Parse("f2250674-42e6-4756-b394-b86caa62225e$1");
            var rosterQuestionId = Id.g1;

            var hqUser = Abc.Create.Entity.HqUser();
            
            this.SetupResponsibleUser(hqUser);
            this.SetupQuestionnaire(Abc.Create.Entity.QuestionnaireDocument(qid.QuestionnaireId, string.Empty,
                children: new IComposite[]
            {
                Abc.Create.Entity.TextQuestion(),
                Abc.Create.Entity.NumericRoster(children: new []
                {
                    Abc.Create.Entity.TextQuestion(rosterQuestionId)
                })
            }));

            var response = this.controller.Create(new CreateAssignmentApiRequest
            {
                QuestionnaireId = qid.ToString(),
                Responsible = hqUser.UserName,
                IdentifyingData = new List<AssignmentIdentifyingDataItem>
                {
                    new AssignmentIdentifyingDataItem
                    {
                        Identity = $"{rosterQuestionId:N}_0",
                        Answer = "text"
                    }
                }
            });
            
            Assert.That(response.Result, Has.Property(nameof(IStatusCodeActionResult.StatusCode)).EqualTo(StatusCodes.Status400BadRequest));
            var verificationErrors = (((ObjectResult) response.Result).Value as CreateAssignmentResult)
                ?.VerificationStatus.Errors;
            
            Assert.That(verificationErrors, Is.Not.Empty);
            Assert.That(verificationErrors?[0].Code, Is.EqualTo("PL0011"));
        }

        [Test]
        public void should_store_new_assignment()
        {
            var qid = QuestionnaireIdentity.Parse("f2250674-42e6-4756-b394-b86caa62225e$1");

            var hqUser = Abc.Create.Entity.HqUser();
            this.SetupResponsibleUser(hqUser);
            this.SetupQuestionnaire(Abc.Create.Entity.QuestionnaireDocumentWithOneQuestion());

            var assignment = Abc.Create.Entity.Assignment(1, qid);

            this.mapper
                .Setup(m => m.Map(It.IsAny<CreateAssignmentApiRequest>(), It.IsAny<Assignment>()))
                .Returns(assignment);
            this.mapper
                .Setup(m => m.Map<AssignmentDetails>(It.IsAny<Assignment>()))
                .Returns(new AssignmentDetails());

            var result = this.controller.Create(new CreateAssignmentApiRequest
            {
                QuestionnaireId = qid.ToString(),
                Responsible = hqUser.UserName
            });

            this.commandService.Verify(ass => ass.Execute(It.IsAny<CreateAssignment>(), null), Times.Once);
            Assert.That(result.Result, Has.Property(nameof(StatusCodeResult.StatusCode)).EqualTo(StatusCodes.Status201Created));
        }

        [Test]
        public void and_quantity_minus_1_should_stored_new_assignment_has_null_quantity()
        {
            var qid = QuestionnaireIdentity.Parse("f2250674-42e6-4756-b394-b86caa62225e$1");

            var hqUser = Abc.Create.Entity.HqUser();
            
            this.SetupResponsibleUser(hqUser);
            this.SetupQuestionnaire(Abc.Create.Entity.QuestionnaireDocument());

            var assignment = Abc.Create.Entity.Assignment(1, qid);

            this.mapper
                .Setup(m => m.Map(It.IsAny<CreateAssignmentApiRequest>(), It.IsAny<Assignment>()))
                .Returns(assignment);
            this.mapper
                .Setup(m => m.Map<AssignmentDetails>(It.IsAny<Assignment>()))
                .Returns(new AssignmentDetails());

            var result = this.controller.Create(new CreateAssignmentApiRequest
            {
                QuestionnaireId = qid.ToString(),
                Responsible = hqUser.UserName,
                Quantity = -1
            });

            this.commandService.Verify(ass => ass.Execute(It.Is<CreateAssignment>(x=>x.Quantity == null), null), Times.Once);
            Assert.That(result.Result, Has.Property(nameof(StatusCodeResult.StatusCode)).EqualTo(StatusCodes.Status201Created));
        }
        
        [Test]
        public void when_assignment_has_invalid_identifying_data_then_should_return_verification_errors()
        {
            var qid = QuestionnaireIdentity.Parse("f2250674-42e6-4756-b394-b86caa62225e$1");

            var hqUser = Abc.Create.Entity.HqUser();
            
            this.SetupResponsibleUser(hqUser);
            this.SetupQuestionnaire(Abc.Create.Entity.QuestionnaireDocument(qid.QuestionnaireId, children: new IComposite[]
            {
                Abc.Create.Entity.NumericQuestion(variableName: "dbl"),
                Abc.Create.Entity.NumericQuestion(variableName: "int", isInteger: true),
                Abc.Create.Entity.DateTimeQuestion(variable: "dt"),
                Abc.Create.Entity.SingleQuestion(variable: "single"),
                Abc.Create.Entity.MultyOptionsQuestion(variable: "multi"),
                Abc.Create.Entity.GpsCoordinateQuestion(variable: "gps")
            }));

            var response = this.controller.Create(new CreateAssignmentApiRequest
            {
                QuestionnaireId = qid.ToString(),
                Responsible = hqUser.UserName,
                WebMode = true,
                Quantity = -1,
                Email = "invalid email",
                IdentifyingData = new List<AssignmentIdentifyingDataItem>
                {
                    new AssignmentIdentifyingDataItem{ Variable = "dbl", Answer = "invalid"},
                    new AssignmentIdentifyingDataItem{ Variable = "int", Answer = "invalid"},
                    new AssignmentIdentifyingDataItem{ Variable = "dt", Answer = "invalid"},
                    new AssignmentIdentifyingDataItem{ Variable = "single", Answer = "invalid"},
                    new AssignmentIdentifyingDataItem{ Variable = "multi", Answer = "['invalid']"},
                    new AssignmentIdentifyingDataItem{ Variable = "gps", Answer = "invalid$"},
                }
            });
            
            Assert.That(response.Result, Has.Property(nameof(IStatusCodeActionResult.StatusCode)).EqualTo(StatusCodes.Status400BadRequest));
            var verificationErrors = (((ObjectResult) response.Result).Value as CreateAssignmentResult)
                ?.VerificationStatus.Errors;

            Assert.That(verificationErrors, Is.Not.Null);
            Assert.That(verificationErrors.Select(x => x.Code),
                Is.EquivalentTo(new[] {"PL0014", "PL0016", "PL0017", "PL0018", "PL0019", "PL0055", "PL0057"}));
        }

        [Test]
        [TestCase(QuestionType.Multimedia, "test")]
        [TestCase(QuestionType.Audio, "test")]
        [TestCase(QuestionType.Area, "test")]
        [TestCase(QuestionType.SingleOption, "test", true)]
        [TestCase(QuestionType.MultyOption, "['test']", true)]
        public void when_assignment_has_not_supported_question_in_identifying_data_then_should_return_verification_errors(QuestionType questionType, string preloadingValue, bool linked = false)
        {
            var variableName = "testQuestion";
            var qid = QuestionnaireIdentity.Parse("f2250674-42e6-4756-b394-b86caa62225e$1");

            var hqUser = Abc.Create.Entity.HqUser();

            this.SetupResponsibleUser(hqUser);
            
            this.SetupQuestionnaire(Abc.Create.Entity.QuestionnaireDocument(qid.QuestionnaireId, children: new IComposite[]
            {
                GetQuestionByType(questionType, variableName, linked? Guid.NewGuid(): (Guid?) null)
            }));

            var response = this.controller.Create(new CreateAssignmentApiRequest
            {
                QuestionnaireId = qid.ToString(),
                Responsible = hqUser.UserName,
                Quantity = -1,
                IdentifyingData = new List<AssignmentIdentifyingDataItem>
                {
                    new AssignmentIdentifyingDataItem{ Variable = variableName, Answer = preloadingValue},
                }
            });

            Assert.That(response.Result, Has.Property(nameof(IStatusCodeActionResult.StatusCode)).EqualTo(StatusCodes.Status400BadRequest));
            var verificationErrors = (((ObjectResult)response.Result).Value as CreateAssignmentResult)
                ?.VerificationStatus.Errors;

            Assert.That(verificationErrors, Is.Not.Null);
            Assert.That(verificationErrors.Select(x => x.Code),
                Is.EquivalentTo(new[] { "PL0063" }));
        }

        private IQuestion GetQuestionByType(QuestionType questionType, string variableName, Guid? linked = null)
        {
            return questionType switch
            {
                QuestionType.Multimedia => Abc.Create.Entity.MultimediaQuestion(variable: variableName),
                QuestionType.QRBarcode => Abc.Create.Entity.QRBarcodeQuestion(variable: variableName),
                QuestionType.Area => Abc.Create.Entity.GeographyQuestion(variable: variableName),
                QuestionType.Audio => Abc.Create.Entity.AudioQuestion(variable: variableName),
                QuestionType.DateTime => Abc.Create.Entity.DateTimeQuestion(variable: variableName),
                QuestionType.GpsCoordinates => Abc.Create.Entity.GpsCoordinateQuestion(variable: variableName),
                QuestionType.MultyOption => Abc.Create.Entity.MultipleOptionsQuestion(variable: variableName, linkedToQuestionId: linked),
                QuestionType.SingleOption => Abc.Create.Entity.SingleOptionQuestion(variable: variableName, linkedToQuestionId: linked),
                QuestionType.Text => Abc.Create.Entity.TextListQuestion(variable: variableName),
                QuestionType.Numeric => Abc.Create.Entity.NumericQuestion(variableName: variableName),
                _ => null
            };
        }
    }
}
