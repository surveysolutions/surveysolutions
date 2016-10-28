using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionDataParserTests
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
        protected static AbstractAnswer result;
        protected static Guid questionId = Guid.NewGuid();
        protected static string questionVarName = "var_a";
        protected static string answer;
        protected static object parcedValue;
        protected static ValueParsingResult parsingResult;
        protected static IQuestion question;
        protected static AbstractAnswer parsedSingleColumnAnswer;
    }
}
