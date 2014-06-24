using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.QuestionDataParserTests
{
    [Subject(typeof (QuestionDataParser))]
    internal class QuestionDataParserTestContext
    {
        protected static QuestionDataParser CreateQuestionDataParser()
        {
            return new QuestionDataParser();
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocumentWithOneChapter(params IComposite[] chapterChildren)
        {
            return new QuestionnaireDocument
            {
                Title = "some title",
                Children = new List<IComposite>
                {
                    new Group("Chapter")
                    {
                        PublicKey = Guid.Parse("FFF000AAA111EE2DD2EE111AAA000FFF"),
                        Children = chapterChildren.ToList(),
                        IsRoster = false
                    }
                }
            };
        }

        protected static QuestionDataParser questionDataParser;
        protected static KeyValuePair<Guid, object>? result;
        protected static Guid questionId = Guid.NewGuid();
        protected static string questionVarName = "var";
        protected static string answer;
        protected static KeyValuePair<Guid, object> parcedValue;
        protected static ValueParsingResult parsingResult;
        protected static IQuestion question;
    }
}
