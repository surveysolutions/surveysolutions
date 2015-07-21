using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionDataParserTests
{
    internal class when_pasing_answer_on_multimedia_question : QuestionDataParserTestContext
    {
        Establish context = () =>
        {
            questionDataParser = CreateQuestionDataParser();
            question = new MultimediaQuestion()
            {
                PublicKey = Guid.NewGuid(),
                StataExportCaption = questionVarName
            };
        };

        Because of =
            () =>
                parsingResult =
                    questionDataParser.TryParse("some answer",questionVarName, question, CreateQuestionnaireDocumentWithOneChapter(question), out parcedValue);

        It should_result_be_UnsupportedLinkedQuestion = () =>
            parsingResult.ShouldEqual(ValueParsingResult.UnsupportedMultimediaQuestion);
    }
}
