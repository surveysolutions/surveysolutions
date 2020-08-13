using System;
using System.Collections.Generic;
using Main.Core.Entities.Composite;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Enumerator.Native.WebInterview.Services;
using WB.Tests.Abc;

using Identity = WB.Core.SharedKernels.DataCollection.Identity;

namespace WB.Tests.Unit.Applications.Headquarters.WebInterview
{
    [TestOf(typeof(WebNavigationService))]
    internal class WebNavigationServiceTests
    {
        [TestCase("cover")]
        [TestCase("complete")]
        public void when_MakeNavigationLinks_with_system_variables(string systemVariable)
        {
            // arrange
            var questionTitleFormat = "<a href=\"{0}\">some text</a>";
            var virtualDirectoryName = "dir";
            var questionId = Guid.Parse("11111111111111111111111111111111");
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneQuestion();
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);

            var interview = SetUp.StatefulInterview(questionnaireDocument);
            var webNavigationService = Web.Create.Service.WebNavigationService();

            // act
            var textWithLink = webNavigationService.MakeNavigationLinks(string.Format(questionTitleFormat, systemVariable),
                Identity.Create(questionId, RosterVector.Empty), questionnaire, interview, virtualDirectoryName);

            // assert
            Assert.That(textWithLink, Is.EqualTo(string.Format(questionTitleFormat, $"~/{virtualDirectoryName}/{interview.Id.FormatGuid()}/{systemVariable}")));
        }

        [Test]
        public void when_MakeNavigationLinks_with_link_to_variable()
        {
            // arrange
            var variableName = "myvar";
            var questionTitleFormat = "<a href=\"{0}\">some text</a>";
            var questionnaireTitle = string.Format(questionTitleFormat, variableName);
            var questionId = Guid.Parse("11111111111111111111111111111111");
            var virtualDirectoryName = "dir";
            var questionnaireDocument = Create.Entity.QuestionnaireDocument(children: new IComposite[]
            {
                Create.Entity.TextQuestion(text: questionnaireTitle),
                Create.Entity.Variable(variableName: variableName)
            });

            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);

            var interview = SetUp.StatefulInterview(questionnaireDocument);
            var webNavigationService = Web.Create.Service.WebNavigationService();

            // act
            var textWithLink = webNavigationService.MakeNavigationLinks(questionnaireTitle,
                Identity.Create(questionId, RosterVector.Empty), questionnaire, interview, virtualDirectoryName);

            // assert
            Assert.That(textWithLink, Is.EqualTo(string.Format(questionTitleFormat, "javascript:void(0);")));
        }

        [Test]
        public void when_MakeNavigationLinks_with_link_to_assignment()
        {
            // arrange
            var attachment = new Core.SharedKernels.SurveySolutions.Documents.Attachment
            {
                Name = "attname1",
                AttachmentId = Guid.Parse("22222222222222222222222222222222"),
                ContentId = "somecontenthash1"
            };
            var questionTitleFormat = "<a href=\"{0}\">some text</a>";
            var questionnaireTitle = string.Format(questionTitleFormat, attachment.Name);
            var questionId = Guid.Parse("11111111111111111111111111111111");
            var virtualDirectoryName = "dir";
            var questionnaireDocument = Create.Entity.QuestionnaireDocument(children: Create.Entity.TextQuestion(text: questionnaireTitle));

            questionnaireDocument.Attachments =
                new List<Core.SharedKernels.SurveySolutions.Documents.Attachment>(new[] {attachment});

            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);

            var interview = SetUp.StatefulInterview(questionnaireDocument);
            var webNavigationService = Web.Create.Service.WebNavigationService();

            // act
            var textWithLink = webNavigationService.MakeNavigationLinks(questionnaireTitle,
                Identity.Create(questionId, RosterVector.Empty), questionnaire, interview, virtualDirectoryName);

            // assert
            Assert.That(textWithLink, Is.EqualTo(string.Format(questionTitleFormat,
                $"~/api/WebInterviewResources/Content?interviewId={interview.Id.FormatGuid()}&contentId={attachment.ContentId}")));
        }

        [Test]
        public void when_MakeNavigationLinks_with_link_to_question()
        {
            // arrange
            var questionId = Guid.Parse("11111111111111111111111111111111");
            var navigateToQuestionId = Guid.Parse("22222222222222222222222222222222");
            var parentGroupId = Guid.Parse("33333333333333333333333333333333");
            var navigateToQuestionVariable = "navigated";
            var questionTitleFormat = "<a href=\"{0}\">some text</a>";
            var questionTitle = string.Format(questionTitleFormat, navigateToQuestionVariable);
            var virtualDirectoryName = "dir";
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
            {
                Create.Entity.Group(parentGroupId, children: new []
                {
                    Create.Entity.TextQuestion(questionId, text: questionTitle),
                    Create.Entity.TextQuestion(navigateToQuestionId, variable: navigateToQuestionVariable),
                })
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);

            var interview = SetUp.StatefulInterview(questionnaireDocument);
            var webNavigationService = Web.Create.Service.WebNavigationService();

            // act
            var textWithLink = webNavigationService.MakeNavigationLinks(questionTitle,
                Identity.Create(questionId, RosterVector.Empty), questionnaire, interview, virtualDirectoryName);

            // assert
            Assert.That(textWithLink, Is.EqualTo(string.Format(questionTitleFormat,
                $"~/{virtualDirectoryName}/{interview.Id.FormatGuid()}/Section/{parentGroupId.FormatGuid()}#{navigateToQuestionId.FormatGuid()}")));
        }

        [Test]
        public void when_MakeNavigationLinks_with_link_to_roster_then_link_should_contain_url_to_first_roster_instance()
        {
            // arrange
            var questionId = Guid.Parse("11111111111111111111111111111111");
            var navigateToQuestionId = Guid.Parse("22222222222222222222222222222222");
            var parentGroupId = Guid.Parse("33333333333333333333333333333333");
            var firstRosterInstanceId = Identity.Create(Guid.Parse("44444444444444444444444444444444"), new RosterVector(new[] {0}));
            var rosterName = "r1";
            var questionTitleFormat = "<a href=\"{0}\">some text</a>";
            var questionTitle = string.Format(questionTitleFormat, rosterName);
            var virtualDirectoryName = "dir";
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
            {
                Create.Entity.Group(parentGroupId, children: new IComposite[]
                {
                    Create.Entity.TextQuestion(questionId, text: questionTitle),
                    Create.Entity.Roster(firstRosterInstanceId.Id, variable: rosterName, children: new []
                    {
                        Create.Entity.TextQuestion(navigateToQuestionId),
                    })
                })
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);

            var interview = SetUp.StatefulInterview(questionnaireDocument);
            var webNavigationService = Web.Create.Service.WebNavigationService();

            // act
            var textWithLink = webNavigationService.MakeNavigationLinks(questionTitle,
                Identity.Create(questionId, RosterVector.Empty), questionnaire, interview, virtualDirectoryName);

            // assert
            Assert.That(textWithLink, Is.EqualTo(string.Format(questionTitleFormat,
                $"~/{virtualDirectoryName}/{interview.Id.FormatGuid()}/Section/{parentGroupId.FormatGuid()}#{firstRosterInstanceId}")));
        }

        [Test]
        public void when_MakeNavigationLinks_with_link_to_identifying_question_then_link_should_contain_url_to_cover_page()
        {
            // arrange
            var questionId = Guid.Parse("11111111111111111111111111111111");
            var identifyingToQuestionId = Guid.Parse("22222222222222222222222222222222");
            var parentGroupId = Guid.Parse("33333333333333333333333333333333");
            var identifyingQuestionVariable = "navigated";
            var questionTitleFormat = "<a href=\"{0}\">some text</a>";
            var questionTitle = string.Format(questionTitleFormat, identifyingQuestionVariable);
            var virtualDirectoryName = "dir";
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.TextQuestion(identifyingToQuestionId, variable: identifyingQuestionVariable, preFilled: true),
                Create.Entity.Group(parentGroupId, children: new []
                {
                    Create.Entity.TextQuestion(questionId, text: questionTitle)
                })
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);

            var interview = SetUp.StatefulInterview(questionnaireDocument);
            var webNavigationService = Web.Create.Service.WebNavigationService();

            // act
            var textWithLink = webNavigationService.MakeNavigationLinks(questionTitle,
                Identity.Create(questionId, RosterVector.Empty), questionnaire, interview, virtualDirectoryName);

            // assert
            Assert.That(textWithLink, Is.EqualTo(string.Format(questionTitleFormat, $"~/{virtualDirectoryName}/{interview.Id.FormatGuid()}/Cover")));
        }
    }
}
