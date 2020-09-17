using System.Linq;
using Main.Core.Documents;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Enumerator.Native.WebInterview.Models;
using WB.Tests.Abc;


namespace WB.Tests.Unit.Applications.Headquarters.WebInterview.Review.Api
{
    public class when_get_interview_details_with_markdown_links : WebInterviewInterviewEntityFactorySpecification
    {
        private const  string targetTextQuestionVariable = "markdownVariable";

        private static readonly string actualTextWithMarkdownLink = $"[hello]({targetTextQuestionVariable})";
        private string expectedTextWithMarkdownLink =>
            $"<a href=\"~/Interview/Review/{CurrentInterview.Id.FormatGuid()}/Section/{Id.gA.FormatGuid()}#{targetQuestionIdentity.Id.FormatGuid()}\">hello</a>";

        private static readonly Identity textQuestionIdentity = Id.Identity1;
        private static readonly Identity staticTextIdentity = Id.Identity2;
        private static readonly Identity targetQuestionIdentity = Id.Identity3;

        private InterviewTextQuestion interviewTextQuestion;
        private InterviewStaticText interviewStaticText;


        protected override QuestionnaireDocument GetDocument()
        {
            return Create.Entity.QuestionnaireDocumentWithOneChapter(Id.gA,
                Create.Entity.TextQuestion(textQuestionIdentity.Id, text: actualTextWithMarkdownLink, instruction: actualTextWithMarkdownLink, validationMessage: actualTextWithMarkdownLink),
                Create.Entity.StaticText(staticTextIdentity.Id, text: actualTextWithMarkdownLink, validationConditions: Create.Entity.ValidationCondition(message: actualTextWithMarkdownLink).ToEnumerable().ToList()),
                Create.Entity.TextQuestion(targetQuestionIdentity.Id, variable: targetTextQuestionVariable));
        }

        protected override void Because()
        {
            // act
            this.MarkQuestionAsInvalid(textQuestionIdentity);
            this.MarkStaticTextAsInvalid(staticTextIdentity);
            this.interviewStaticText = Subject.GetEntityDetails(staticTextIdentity.ToString(), CurrentInterview, questionnaire, true) as InterviewStaticText;
            this.interviewTextQuestion = Subject.GetEntityDetails(textQuestionIdentity.ToString(), CurrentInterview, questionnaire, true) as InterviewTextQuestion;
        }

        [Test]
        public void should_get_text_question_details_with_markdown_link()
        {
            Assert.That(interviewTextQuestion.Title, Is.EqualTo(expectedTextWithMarkdownLink));
            Assert.That(interviewTextQuestion.Instructions, Is.EqualTo(expectedTextWithMarkdownLink));
            Assert.That(interviewTextQuestion.Validity.Messages[0], Is.EqualTo(expectedTextWithMarkdownLink));
        }

        [Test]
        public void should_get_static_text_details_with_markdown_link()
        {
            Assert.That(interviewStaticText.Title, Is.EqualTo(expectedTextWithMarkdownLink));
            Assert.That(interviewStaticText.Validity.Messages[0], Is.EqualTo(expectedTextWithMarkdownLink));
        }
    }
}
